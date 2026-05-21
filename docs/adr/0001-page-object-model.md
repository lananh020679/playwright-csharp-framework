# 0001 - Use Page Object Model with Fluent Navigation

**Status**: Accepted
**Date**: 2026-05-20

## Context

Tests need to interact with multiple pages: login, employee list, employee
form. Two common approaches:
1. Raw Playwright locators in test methods
2. Page Object Model (POM) abstracting pages as classes
3. Worker/Screenplay pattern with actor-task separation

## Decision

Use Page Object Model with fluent navigation (each method returns the
destination page).

## Rationale

- POM is the most widely understood pattern → easier onboarding for any C#
  test engineer
- Fluent navigation makes tests read like user stories
- Worker pattern adds complexity without payoff at current project size
  (~10-20 tests)

## Consequences

**Positive:**
- Test code stays declarative ("login → go to list → click create")
- Selector changes localized to one Page Object class
- Easy to extend for new pages

**Negative:**
- Page Objects must be maintained alongside the application UI
- Fluent return types create coupling between page navigation flows
  (e.g., if "Submit" can go to multiple destinations, return type lies)

## Alternatives considered

- **Raw locators**: rejected, leads to selector duplication and test brittleness
- **Worker pattern**: deferred, may revisit if project grows beyond 50 tests
