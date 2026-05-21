# 0003 - Locale-Independent Selectors

**Status**: Accepted
**Date**: 2026-05-20

## Context

The target app (eaapp.somee.com) ships in English today, but the team works in
multiple locales (NL, EN, VI) and the app may be localized in future. Common
Playwright selector strategies have different sensitivity to UI text:

1. Text-based: `page.GetByText("Login")`, `:has-text("Save")`
2. Role + accessible name: `page.GetByRole(AriaRole.Button, new() { Name = "Login" })`
3. ID-based: `#UserName`, `#Password`
4. Structural CSS: `table tbody tr`, `button.btn.btn-signin`
5. Attribute-based: `a[href='/Account/Logout']`, `[data-testid='submit']`

Options 1 and 2 break the moment a translator changes "Login" to "Inloggen" or
"Đăng nhập". The test was correct, the app still works, but the test fails.

## Decision

Use only **locale-independent** selectors:

- **IDs** — `#UserName`, `#Password`, `#UserName-error`
- **Hrefs / route attributes** — `a[href='/Employee/Create']`,
  `form[action='/Account/Logout']`
- **Structural CSS** — `table tbody tr`, `.validation-summary-errors`
- **Class on semantic element** — `button.btn.btn-signin`

Forbid in this codebase:

- `GetByText(...)`, `:has-text(...)`, `:text-is(...)`
- `GetByRole(..., new() { Name = "..." })` where Name is UI text
- Locator strings containing user-visible labels

## Rationale

- Routes (`/Account/Login`) and IDs (`UserName`) are part of the app contract,
  not the presentation layer — they don't change when the language changes.
- Class names like `btn-signin` describe the semantic role of the button
  (sign-in action), not its label text — still stable across locales.
- A failing test should mean "the app is broken," not "the translator
  changed a button label."

## Consequences

**Positive:**
- Tests survive translation work without modification.
- Locator intent is explicit: `a[href='/Employee/Create']` documents the
  navigation target; `GetByText("Create")` doesn't.
- No coupling between test code and i18n resource files.

**Negative:**
- Requires app developers to keep stable IDs / route paths. If a refactor
  renames `/Employee/Create` → `/Employees/New`, tests break.
- Less resilient to DOM refactors than role-based selectors would be (e.g.,
  changing `<button class="btn-signin">` to `<input type="submit">` breaks
  the selector).
- Slightly harder to write: developers must inspect the DOM and find a
  non-text anchor, instead of using the visible label.

## Alternatives considered

- **`data-testid` attributes**: ideal but requires app cooperation
  (eaapp.somee.com is a third-party demo — we can't add attributes). Adopt if
  testing an in-house app.
- **Role + accessible name**: rejected — `Name` parameter is the accessible
  text, which IS the localized string in most apps.
- **Mixed strategy with fallback**: rejected — encourages developers to
  reach for text selectors "just this once" and erodes the discipline.

## Related

- [[0001-page-object-model]] — selectors live in Page Object classes, so the
  locale-independence rule has a single enforcement point.
