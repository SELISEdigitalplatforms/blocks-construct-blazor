# Action: update-config

## Purpose

Update one or more project configuration settings. Multiple settings can be updated in a single request.

---

## Endpoint

```
POST $API_BASE_URL/utilities/v1/Config/Save
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/Config/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "settings": [
      {
        "key": "max-upload-size",
        "value": "20971520"
      },
      {
        "key": "session-timeout",
        "value": "7200"
      }
    ],
    "projectKey": "'"$PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| settings | array | yes | List of key/value pairs to update |
| settings[].key | string | yes | Configuration key name — must match an existing key from `get-config` |
| settings[].value | string | yes | New value for the key |
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The configuration settings are updated immediately.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "settings": "One or more keys are invalid"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Invalid key or value — inspect `errors` for details | Verify keys from `get-config`; check value formats |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Always load the current settings via `get-config` before updating to ensure you are modifying valid keys.
- Multiple settings can be updated atomically in a single request.
- After successful update, invalidate the `['config']` React Query cache to refresh the displayed values.
- Configuration changes take effect immediately — no restart or redeployment is required.
