# Flow: user-onboarding-with-notification

## Trigger

User wants to create a new user account, assign them roles, and send a welcome email or notification — all in one operation.

> "create a user and send them a welcome email"
> "onboard a new team member with admin role and welcome notification"
> "add user, set role to manager, and notify them"

---

## Pre-flight Questions

Before starting, confirm:

1. What are the user's details — email, name, password (or should they set their own)?
2. Which role(s) should be assigned?
3. Should a welcome email be sent, an in-app notification, or both?
4. If email: which template purpose to use? Or ad-hoc subject/body?
5. Is this a self-activation flow or admin-activated?

---

## Cross-Domain Dependencies

This flow spans three domains in this order:

| Step | Domain | Action |
|------|--------|--------|
| 1 | identity-access | Create user account |
| 2 | identity-access | Assign roles to user |
| 3 | identity-access | Activate user (if admin-activated) |
| 4 | communication | Send welcome email (template or ad-hoc) |
| 5 | communication | Send in-app notification (optional) |

---

## Flow Steps

### Step 1 — Create User

```
Action: identity-access/actions/create-user
Input:
  email       = user@example.com
  firstName   = John
  lastName    = Doe
  password    = (user-provided or auto-generated)
  projectKey  = $PROJECT_SLUG
Output:
  userId → needed for Steps 2-5
```

On `isSuccess: true` → extract `userId` from response.

### Step 2 — Assign Roles

```
Action: identity-access/actions/set-roles
Input:
  userId     = (from Step 1)
  roles      = ["manager"]
  projectKey = $PROJECT_SLUG
```

### Step 3 — Activate User (if needed)

```
Action: identity-access/actions/activate-user
Input:
  userId     = (from Step 1)
  projectKey = $PROJECT_SLUG
```

Skip if the user self-activates via email link.

### Step 4 — Send Welcome Email

```
Action: communication/actions/send-email-with-template
Input:
  userId          = (from Step 1)
  purpose         = "welcome"
  language        = "en"
  bodyDataContext  = { "name": "John", "role": "Manager" }
  projectKey      = $PROJECT_SLUG
```

If no template exists, use `communication/actions/send-email-to-any` with ad-hoc subject/body.

### Step 5 — Send In-App Notification (Optional)

```
Action: communication/actions/send-notification
Input:
  userIds              = [(from Step 1)]
  denormalizedPayload  = '{"message": "Welcome to the platform!", "type": "welcome"}'
  projectKey           = $PROJECT_SLUG
```

---

## Error Handling

| Error | Step | Action |
|-------|------|--------|
| User creation fails (409) | Step 1 | User already exists — fetch existing user and continue from Step 2 |
| Role assignment fails | Step 2 | Verify role names exist — check with get-roles first |
| Email send fails | Step 4 | Non-critical — log warning, user is still created |
| 401 on any step | Any | Refresh token and retry the failed step only |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/identity-access/pages/user-onboarding/user-onboarding-page.tsx` | Multi-step form: user details → role selection → notification preferences → confirm |
| `modules/identity-access/components/onboarding-stepper/onboarding-stepper.tsx` | Step indicator component |
