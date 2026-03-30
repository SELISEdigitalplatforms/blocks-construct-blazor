# Action: get-config

## Purpose

Retrieve all configuration settings for the project. Use this to display current settings and to identify which keys can be updated.

---

## Endpoint

```
GET $API_BASE_URL/utilities/v1/Config/Gets
```

---

## curl

```bash
curl --location "$API_BASE_URL/utilities/v1/Config/Gets?projectKey=$PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| projectKey | string | yes | Use `$PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "settings": [
    {
      "key": "max-upload-size",
      "value": "10485760",
      "description": "Maximum file upload size in bytes"
    },
    {
      "key": "session-timeout",
      "value": "3600",
      "description": "Session timeout in seconds"
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Missing `projectKey` | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- All configuration settings for the project are returned in a single response.
- Use the `key` from each setting to construct an `UpdateConfigRequest` when updating values.
- The `description` field provides human-readable context for each setting — display this in the UI as helper text.
