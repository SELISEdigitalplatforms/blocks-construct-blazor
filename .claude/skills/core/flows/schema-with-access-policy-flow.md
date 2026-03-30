# Flow: schema-with-access-policy

## Trigger

User wants to define a data schema with access policies and validation rules in one operation.

> "create a schema with role-based access"
> "define a users table with read/write policies"
> "set up a data model with validation and access control"

---

## Pre-flight Questions

1. What is the schema/collection name?
2. What fields does it need? (name, type, required?)
3. Which roles should have read access? Write access?
4. Are there validation rules for any fields?

---

## Cross-Domain Dependencies

| Step | Domain | Action |
|------|--------|--------|
| 1 | data-management | Define schema |
| 2 | data-management | Save schema fields |
| 3 | data-management | Create access policy |
| 4 | data-management | Create validation rules |

---

## Flow Steps

### Step 1 — Define Schema

```
Action: data-management/actions/define-schema
Input:
  name         = "users"
  description  = "User profiles collection"
  projectKey   = $PROJECT_SLUG
Output:
  schemaId → needed for Steps 2-4
```

### Step 2 — Save Schema Fields

```
Action: data-management/actions/save-schema-fields
Input:
  schemaId = (from Step 1)
  fields   = [
    { name: "email", type: "string", required: true },
    { name: "displayName", type: "string", required: true },
    { name: "age", type: "number", required: false }
  ]
  projectKey = $PROJECT_SLUG
```

### Step 3 — Create Access Policy

```
Action: data-management/actions/create-access-policy
Input:
  schemaId    = (from Step 1)
  rules       = [
    { role: "admin", operations: ["read", "write", "delete"] },
    { role: "user", operations: ["read"] }
  ]
  projectKey  = $PROJECT_SLUG
```

### Step 4 — Create Validation Rules (Optional)

```
Action: data-management/actions/create-validation
Input:
  schemaId = (from Step 1)
  rules    = [
    { field: "email", rule: "email", message: "Must be a valid email" },
    { field: "age", rule: "range", min: 0, max: 150, message: "Age must be 0-150" }
  ]
  projectKey = $PROJECT_SLUG
```

---

## Error Handling

| Error | Step | Action |
|-------|------|--------|
| Schema already exists (409) | Step 1 | Fetch existing schema, continue from Step 2 |
| Invalid field type | Step 2 | Check contracts.md for valid field types |
| Access policy conflict | Step 3 | Fetch existing policies, update instead of create |
| 401 on any step | Any | Refresh token and retry failed step |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/schema-wizard/schema-wizard-page.tsx` | Multi-step wizard: schema info → fields → access policies → validation → confirm |
| `modules/data-management/components/field-editor/field-editor.tsx` | Dynamic field list editor |
| `modules/data-management/components/access-policy-editor/access-policy-editor.tsx` | Role-permission matrix UI |
