# Known Issues and Limitations

1. **Critical security**: plaintext-looking Gemini key in `services/api/appsettings.json`; redact/rotate.
2. **High security**: payment completion is broadcast to all SignalR clients.
3. **High security/compatibility**: legacy plaintext password fallback remains.
4. **Medium**: AI permission service checks role/risk but branch isolation is not a single central gate.
5. **Medium**: Product, Area, Topping and Promotion do not consistently model branch ownership.
6. **Medium**: promotion CRUD is present, but applying promotions to order totals is not verified.
7. **Medium**: QR/table identifiers originate in the client; server validation must remain authoritative.
8. **Unknown/operational**: active migration history is ambiguous because `src`, top-level `Infrastructure`, and backup migration trees coexist.
9. **Unknown**: production hosting/provider and production deployment state are not proven.
10. **Planned/unknown**: ingredient/inventory module is not present as a verified current end-to-end feature.
11. **Partial**: AI conversation history is request-provided; persistent chat history is not verified.
12. **Quality**: current working tree is dirty and contains user changes; build/test results must be recorded separately from this static audit.
