using System;

namespace RestaurantPOS.Domain.Entities;

public class ReceiptSettings
{
    public const string InvoiceDocumentType = "Invoice";
    public const string StockReceiptDocumentType = "StockReceipt";

    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string DocumentType { get; set; } = InvoiceDocumentType;
    public int PaperWidth { get; set; } = 80;
    public bool ShowLogo { get; set; } = true;
    public bool ShowAddress { get; set; } = true;
    public bool ShowPhone { get; set; } = true;
    public bool ShowStaff { get; set; } = true;
    public bool ShowPaymentMethod { get; set; } = true;
    public bool ShowOrderNote { get; set; } = true;
    public bool ShowThankYou { get; set; } = true;
    public bool ShowSupplier { get; set; } = true;
    public bool ShowWarehouse { get; set; } = true;
    public bool ShowUnit { get; set; } = true;
    public bool ShowUnitPrice { get; set; } = true;
    public bool ShowPreparedBy { get; set; } = true;
    public bool ShowApprovedBy { get; set; } = false;
    public bool ShowSignatures { get; set; } = true;
    public string ThankYouText { get; set; } = "Cảm ơn quý khách và hẹn gặp lại!";
    public string FontFamily { get; set; } = "font-mono";
    public int FontSize { get; set; } = 11;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
