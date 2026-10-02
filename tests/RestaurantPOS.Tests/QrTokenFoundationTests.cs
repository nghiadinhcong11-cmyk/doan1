using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantPOS.Application.Common.Security;
using RestaurantPOS.Application.DTOs;
using RestaurantPOS.Application.Services;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.WebAPI.Controllers;

namespace RestaurantPOS.Tests;

public sealed class QrTokenFoundationTests
{
    [Fact]
    public void Generator_creates_a_32_byte_url_safe_base64_token()
    {
        var token = QrTokenGenerator.Generate();

        Assert.Equal(QrTokenGenerator.EncodedLength, token.Length);
        Assert.DoesNotContain('+', token);
        Assert.DoesNotContain('/', token);
        Assert.DoesNotContain('=', token);
    }

    [Fact]
    public void Generator_creates_distinct_tokens()
    {
        Assert.NotEqual(QrTokenGenerator.Generate(), QrTokenGenerator.Generate());
    }

    [Fact]
    public void Backfill_preparation_dry_run_only_assigns_null_tokens_without_mutating_source_rows()
    {
        var nullTokenTable = new RestaurantTable { Id = Guid.NewGuid() };
        var existingTokenTable = new RestaurantTable { Id = Guid.NewGuid(), QrToken = QrTokenGenerator.Generate() };
        var existingToken = existingTokenTable.QrToken;

        var assignments = QrTokenBackfillPreparation.CreateAssignments(new[] { nullTokenTable, existingTokenTable });

        var assignment = Assert.Single(assignments);
        Assert.Equal(nullTokenTable.Id, assignment.TableId);
        Assert.Equal(QrTokenGenerator.EncodedLength, assignment.Token.Length);
        Assert.Equal(existingToken, existingTokenTable.QrToken);
        Assert.Null(nullTokenTable.QrToken);
    }

    [Fact]
    public void Backfill_preparation_is_empty_after_rows_are_populated()
    {
        var table = new RestaurantTable { Id = Guid.NewGuid(), QrToken = QrTokenGenerator.Generate() };

        Assert.Empty(QrTokenBackfillPreparation.CreateAssignments(new[] { table }));
    }

    [Fact]
    public void Backfill_target_guard_requires_explicit_matching_project_and_connection_identity()
    {
        Assert.True(QrTokenBackfillTargetGuard.IsAuthorized(
            QrTokenBackfillTargetGuard.AuthorizedDevProjectRef,
            "aws-0-ap-southeast-2.pooler.supabase.com",
            "postgres.qfkgjxwbshjgsxsvkpkp"));
        Assert.False(QrTokenBackfillTargetGuard.IsAuthorized(
            "wrong-project",
            "aws-0-ap-southeast-2.pooler.supabase.com",
            "postgres.qfkgjxwbshjgsxsvkpkp"));
    }

    [Fact]
    public async Task Table_service_generates_and_overrides_client_supplied_token()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var branch = new Branch { Id = Guid.NewGuid(), Name = "QR Branch" };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var service = new TableService(context);
        var first = await service.CreateTableAsync(new RestaurantTable
        {
            Name = "QR Table 1",
            AreaName = "Main",
            BranchId = branch.Id,
            QrToken = "client-controlled"
        }, null);
        var second = await service.CreateTableAsync(new RestaurantTable
        {
            Name = "QR Table 2",
            AreaName = "Main",
            BranchId = branch.Id
        }, null);

        Assert.NotNull(first.QrToken);
        Assert.Equal(QrTokenGenerator.EncodedLength, first.QrToken.Length);
        Assert.NotEqual("client-controlled", first.QrToken);
        Assert.NotEqual(first.QrToken, second.QrToken);
    }

    [Fact]
    public async Task Qr_token_model_is_required_bounded_and_uniquely_indexed()
    {
        var (context, connection) = TestDbContextFactory.Create();
        await using var _ = context;
        await using var __ = connection;

        var entityType = context.Model.FindEntityType(typeof(RestaurantTable));
        var property = entityType!.FindProperty(nameof(RestaurantTable.QrToken));
        var index = entityType.GetIndexes().Single(candidate =>
            candidate.Properties.Single().Name == nameof(RestaurantTable.QrToken));

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(QrTokenGenerator.EncodedLength, property.GetMaxLength());
        Assert.True(index.IsUnique);
        Assert.Null(index.GetFilter());
    }

    [Fact]
    public async Task Public_table_response_does_not_expose_qr_token()
    {
        var tableService = new Mock<ITableService>();
        tableService.Setup(service => service.GetTablesAsync(It.IsAny<Guid?>(), null, null, null))
            .ReturnsAsync(new List<RestaurantTable>
            {
                new() { Id = Guid.NewGuid(), Name = "Public Table", AreaName = "Main", QrToken = QrTokenGenerator.Generate() }
            });
        var controller = new TableController(tableService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetTables(null, null, null, Guid.NewGuid());
        var response = Assert.IsAssignableFrom<IEnumerable<TableResponseDto>>(((OkObjectResult)result).Value).Single();

        Assert.Null(typeof(TableResponseDto).GetProperty(nameof(RestaurantTable.QrToken)));
        Assert.Equal("Public Table", response.Name);
    }
}
