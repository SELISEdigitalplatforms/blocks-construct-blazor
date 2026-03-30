# Blocks AI Skills — Index

**This is a lookup index for internal routing. Do not present this list to the user.**
When a user makes a request, use `skills/core/decision.md` to identify which file to fetch, then fetch only that file. Never load all files upfront and never enumerate available skills to the user.

Base URL: `https://raw.githubusercontent.com/rezwanx/blocks-ai-skills/main`

## Core

- skills/core/setup-consuming-project.md
- skills/core/context.md
- skills/core/clarification.md
- skills/core/decision.md
- skills/core/runtime.md
- skills/core/conventions.md
- skills/core/frontend-react.md
- skills/core/frontend-blazor.md
- skills/core/security.md
- skills/core/prerequisites.md
- skills/core/app-scaffold-react.md
- skills/core/app-scaffold-blazor.md
- skills/core/app-layout-react.md
- skills/core/app-layout-blazor.md
- skills/core/frontend-blazor-tailwind.md
- skills/core/app-scaffold-blazor-tailwind.md
- skills/core/app-layout-blazor-tailwind.md

## Identity & Access

- skills/identity-access/SKILL.md
- skills/identity-access/contracts.md
- skills/identity-access/frontend-react.md
- skills/identity-access/frontend-blazor.md
- skills/identity-access/frontend-blazor-tailwind.md

### Flows
- skills/identity-access/flows/login-flow.md
- skills/identity-access/flows/user-registration.md
- skills/identity-access/flows/password-recovery.md
- skills/identity-access/flows/mfa-setup.md
- skills/identity-access/flows/user-onboarding.md
- skills/identity-access/flows/session-management.md
- skills/identity-access/flows/role-permission-setup.md

### Authentication
- skills/identity-access/actions/get-token.md
- skills/identity-access/actions/refresh-token.md
- skills/identity-access/actions/logout.md
- skills/identity-access/actions/logout-all.md
- skills/identity-access/actions/get-user-info.md
- skills/identity-access/actions/get-login-options.md
- skills/identity-access/actions/get-social-login-endpoint.md
- skills/identity-access/actions/generate-user-code.md
- skills/identity-access/actions/get-user-codes.md

### Users
- skills/identity-access/actions/create-user.md
- skills/identity-access/actions/update-user.md
- skills/identity-access/actions/get-users.md
- skills/identity-access/actions/get-user.md
- skills/identity-access/actions/activate-user.md
- skills/identity-access/actions/deactivate-user.md
- skills/identity-access/actions/change-password.md
- skills/identity-access/actions/recover-user.md
- skills/identity-access/actions/reset-password.md
- skills/identity-access/actions/resend-activation.md
- skills/identity-access/actions/validate-activation-code.md
- skills/identity-access/actions/check-email-available.md
- skills/identity-access/actions/get-account.md
- skills/identity-access/actions/get-account-roles.md
- skills/identity-access/actions/get-account-permissions.md
- skills/identity-access/actions/get-user-roles.md
- skills/identity-access/actions/get-user-permissions.md
- skills/identity-access/actions/get-sessions.md
- skills/identity-access/actions/get-histories.md

### Roles
- skills/identity-access/actions/create-role.md
- skills/identity-access/actions/update-role.md
- skills/identity-access/actions/get-roles.md
- skills/identity-access/actions/get-role.md
- skills/identity-access/actions/set-roles.md

### Permissions
- skills/identity-access/actions/create-permission.md
- skills/identity-access/actions/update-permission.md
- skills/identity-access/actions/get-permissions.md
- skills/identity-access/actions/get-permission.md
- skills/identity-access/actions/save-roles-and-permissions.md
- skills/identity-access/actions/get-resource-groups.md

### Organizations
- skills/identity-access/actions/save-organization.md
- skills/identity-access/actions/get-organizations.md
- skills/identity-access/actions/get-organization.md
- skills/identity-access/actions/save-organization-config.md
- skills/identity-access/actions/get-organization-config.md

### MFA
- skills/identity-access/actions/generate-otp.md
- skills/identity-access/actions/verify-otp.md
- skills/identity-access/actions/setup-totp.md
- skills/identity-access/actions/disable-user-mfa.md
- skills/identity-access/actions/resend-otp.md

### Captcha
- skills/identity-access/actions/create-captcha.md
- skills/identity-access/actions/submit-captcha.md
- skills/identity-access/actions/verify-captcha.md

## AI Services

- skills/ai-services/SKILL.md
- skills/ai-services/contracts.md
- skills/ai-services/frontend-react.md
- skills/ai-services/frontend-blazor.md
- skills/ai-services/frontend-blazor-tailwind.md

### Flows
- skills/ai-services/flows/create-agent-flow.md
- skills/ai-services/flows/setup-knowledge-base.md
- skills/ai-services/flows/chat-flow.md
- skills/ai-services/flows/query-lmt-flow.md
- skills/ai-services/flows/manage-models.md

### Agents
- skills/ai-services/actions/create-agent.md
- skills/ai-services/actions/update-agent-persona.md
- skills/ai-services/actions/update-agent-ai-config.md
- skills/ai-services/actions/change-agent-status.md
- skills/ai-services/actions/delete-agent.md
- skills/ai-services/actions/get-agents.md
- skills/ai-services/actions/get-agent.md
- skills/ai-services/actions/publish-agent.md

### Knowledge Base
- skills/ai-services/actions/upload-kb-file.md
- skills/ai-services/actions/ingest-kb-text.md
- skills/ai-services/actions/ingest-kb-qa.md
- skills/ai-services/actions/ingest-kb-link.md
- skills/ai-services/actions/create-kb-folder.md
- skills/ai-services/actions/delete-kb.md
- skills/ai-services/actions/test-kb-retrieval.md

### Tools
- skills/ai-services/actions/create-api-tool.md
- skills/ai-services/actions/create-mcp-tool.md
- skills/ai-services/actions/test-tool-action.md
- skills/ai-services/actions/get-tools.md
- skills/ai-services/actions/delete-tool.md

### Models
- skills/ai-services/actions/create-model.md
- skills/ai-services/actions/get-models.md
- skills/ai-services/actions/get-model.md
- skills/ai-services/actions/validate-model.md
- skills/ai-services/actions/get-provider-models.md

### Conversations & Chat
- skills/ai-services/actions/initiate-conversation.md
- skills/ai-services/actions/get-conversations.md
- skills/ai-services/actions/delete-conversation.md
- skills/ai-services/actions/query-lmt.md
- skills/ai-services/actions/stream-query-lmt.md
- skills/ai-services/actions/chat-agent.md
- skills/ai-services/actions/chat-sse.md

## LMT (Logging, Monitoring & Tracing)

- skills/lmt/SKILL.md
- skills/lmt/contracts.md
- skills/lmt/frontend-react.md
- skills/lmt/frontend-blazor.md
- skills/lmt/frontend-blazor-tailwind.md

### Flows
- skills/lmt/flows/view-logs-flow.md
- skills/lmt/flows/view-traces-flow.md

### Logs
- skills/lmt/actions/get-logs.md
- skills/lmt/actions/get-logs-by-date.md
- skills/lmt/actions/stream-live-logs.md

### Traces
- skills/lmt/actions/get-traces.md
- skills/lmt/actions/get-trace.md
- skills/lmt/actions/get-operational-analytics.md
- skills/lmt/actions/get-service-analytics.md

## Data Management

- skills/data-management/SKILL.md
- skills/data-management/contracts.md
- skills/data-management/frontend-react.md
- skills/data-management/frontend-blazor.md
- skills/data-management/frontend-blazor-tailwind.md

### Flows
- skills/data-management/flows/define-schema-flow.md
- skills/data-management/flows/query-data-flow.md
- skills/data-management/flows/migrate-schema-flow.md
- skills/data-management/flows/setup-data-source-flow.md
- skills/data-management/flows/upload-file-flow.md
- skills/data-management/flows/configure-access-policy-flow.md

### Schema
- skills/data-management/actions/get-schemas.md
- skills/data-management/actions/get-schema.md
- skills/data-management/actions/delete-schema.md
- skills/data-management/actions/define-schema.md
- skills/data-management/actions/update-schema.md
- skills/data-management/actions/save-schema-info.md
- skills/data-management/actions/update-schema-info.md
- skills/data-management/actions/save-schema-fields.md
- skills/data-management/actions/get-unadapted-changes.md
- skills/data-management/actions/get-schemas-aggregation.md
- skills/data-management/actions/get-schema-collections.md
- skills/data-management/actions/get-schema-by-collection.md

### DataSource
- skills/data-management/actions/get-data-source.md
- skills/data-management/actions/add-data-source.md
- skills/data-management/actions/update-data-source.md

### DataAccess
- skills/data-management/actions/change-security.md
- skills/data-management/actions/create-access-policy.md
- skills/data-management/actions/update-access-policy.md
- skills/data-management/actions/delete-access-policy.md
- skills/data-management/actions/get-access-policies.md

### DataValidation
- skills/data-management/actions/get-validations.md
- skills/data-management/actions/create-validation.md
- skills/data-management/actions/update-validation.md
- skills/data-management/actions/get-validation.md
- skills/data-management/actions/delete-validation.md
- skills/data-management/actions/get-schema-validations.md
- skills/data-management/actions/get-field-validation.md

### Files
- skills/data-management/actions/get-file.md
- skills/data-management/actions/get-files.md
- skills/data-management/actions/get-files-info.md
- skills/data-management/actions/get-presigned-upload-url.md
- skills/data-management/actions/delete-file.md
- skills/data-management/actions/upload-to-local-storage.md
- skills/data-management/actions/update-file-info.md
- skills/data-management/actions/get-dms-files.md
- skills/data-management/actions/upload-to-dms.md
- skills/data-management/actions/create-folder.md
- skills/data-management/actions/delete-folder.md

### Configuration
- skills/data-management/actions/reload-configuration.md

### DataManage
- skills/data-management/actions/get-mock-data.md
- skills/data-management/actions/delete-mock-data.md

## Localization

- skills/localization/SKILL.md
- skills/localization/contracts.md
- skills/localization/frontend-react.md
- skills/localization/frontend-blazor.md
- skills/localization/frontend-blazor-tailwind.md

### Flows
- skills/localization/flows/language-setup.md
- skills/localization/flows/key-management.md
- skills/localization/flows/import-export.md

### Languages
- skills/localization/actions/save-language.md
- skills/localization/actions/get-languages.md
- skills/localization/actions/delete-language.md
- skills/localization/actions/set-default-language.md

### Modules
- skills/localization/actions/save-module.md
- skills/localization/actions/get-modules.md

### Keys
- skills/localization/actions/save-key.md
- skills/localization/actions/save-keys.md
- skills/localization/actions/get-keys.md
- skills/localization/actions/get-keys-by-names.md
- skills/localization/actions/get-key.md
- skills/localization/actions/delete-key.md
- skills/localization/actions/get-key-timeline.md
- skills/localization/actions/get-uilm-file.md
- skills/localization/actions/generate-uilm-file.md
- skills/localization/actions/translate-all.md
- skills/localization/actions/translate-key.md
- skills/localization/actions/import-uilm.md
- skills/localization/actions/export-uilm.md
- skills/localization/actions/get-exported-files.md
- skills/localization/actions/get-generation-history.md
- skills/localization/actions/rollback-key.md

### Config
- skills/localization/actions/save-webhook.md

## Communication

- skills/communication/SKILL.md
- skills/communication/contracts.md
- skills/communication/frontend-react.md
- skills/communication/frontend-blazor.md
- skills/communication/frontend-blazor-tailwind.md

### Flows
- skills/communication/flows/send-email-flow.md
- skills/communication/flows/notification-flow.md
- skills/communication/flows/manage-templates-flow.md

### Mail
- skills/communication/actions/send-email-to-any.md
- skills/communication/actions/send-email-with-template.md
- skills/communication/actions/get-mailbox-mails.md
- skills/communication/actions/get-mailbox-mail.md

### Notifier
- skills/communication/actions/send-notification.md
- skills/communication/actions/get-unread-notifications.md
- skills/communication/actions/mark-all-notifications-read.md
- skills/communication/actions/mark-notification-read.md
- skills/communication/actions/get-notifications.md

### Template
- skills/communication/actions/save-template.md
- skills/communication/actions/get-template.md
- skills/communication/actions/get-templates.md
- skills/communication/actions/clone-template.md
- skills/communication/actions/delete-template.md

## Utilities

- skills/utilities/SKILL.md
- skills/utilities/contracts.md
- skills/utilities/frontend-react.md
- skills/utilities/frontend-blazor.md
- skills/utilities/frontend-blazor-tailwind.md

### Flows
- skills/utilities/flows/setup-scheduled-task-flow.md
- skills/utilities/flows/manage-webhooks-flow.md

### ScheduledTask
- skills/utilities/actions/create-scheduled-task.md
- skills/utilities/actions/get-scheduled-tasks.md
- skills/utilities/actions/update-scheduled-task.md
- skills/utilities/actions/delete-scheduled-task.md

### Webhook
- skills/utilities/actions/create-webhook.md
- skills/utilities/actions/get-webhooks.md
- skills/utilities/actions/delete-webhook.md

### Config
- skills/utilities/actions/get-config.md
- skills/utilities/actions/update-config.md
