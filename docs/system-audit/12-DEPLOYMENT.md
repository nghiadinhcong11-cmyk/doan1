# Deployment and Configuration

## Current repository evidence

- Docker files have been removed from the current repository and are not a runtime dependency.
- `Program.cs` requires a configured PostgreSQL connection string, normalizes PostgreSQL/Supabase URI formats, enables retry, runs EF migrations and seeding at startup, and requires an HTTPS certificate.
- Local launch settings use `https://0.0.0.0:5000`; the certificate helper is `scripts/setup-https.ps1` and `.https` is present.
- Frontends are Vite projects with `build` scripts; API base URLs are configured in app source/config files rather than proven deployment manifests.

## Not proven

The repository does not provide sufficient evidence to claim a currently active Render, Supabase-hosted deployment, mobile/Capacitor build, production domain, or production database. Supabase appears in configuration/comments and connection handling, but active external infrastructure cannot be established from source alone.

Classification: direct API/frontend local execution is CURRENT; Docker deployment is REMOVED; production hosting is UNKNOWN; external PostgreSQL provider is CONFIGURED/INTENDED, not independently verified.
