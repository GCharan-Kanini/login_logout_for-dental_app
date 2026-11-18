# Legacy shared-login acceptance record

Template for the record that proves a migration run is complete. Copy it once per clinic or site and per run, and fill it
in as you go (see the runbook, `legacy-shared-login-migration.md`, section 3 and section 5).

Never write a password, a password hash, a token or a session value in this record. The legacy identifier is the only
login detail recorded here.

Fill-in values are written as `<like this>`. Leave a field as `unknown` only where the runbook allows it, and name the
person who will confirm it.

## A. Run header

- run_id: `<run_id>`
- clinic: `<clinic name>`
- site: `<site name>`
- runbook_version: `2`
- started_at: `<YYYY-MM-DDTHH:MM:SSZ>`
- owner_name: `<clinic owner>`

## B. Legacy-data entry (label: LEGACY)

Recorded once, before any account is created. Its label must be exactly `LEGACY`.

- entry_label: `LEGACY`
- recorded_at: `<YYYY-MM-DDTHH:MM:SSZ>`
- legacy_identifier: `<shared login identifier, as typed at sign-in>`
- last_known_contact: `<address the shared login sends mail to, or unknown>`
- contact_unknown_responsible: `<name of the person finding it, if unknown>`
- approximate_usage: `<sites, roles, rough number of people, times of day; say these are estimates>`
- sites_sharing_login: `<every site that signs in with this login>`
- captured_by: `<name>`

## C. Per-staff provisioning checklist

One row for each intended user. Count the rows against the staff list you made in section 1 of the runbook.

| staff_ref | full_name | work_email | site | role | account_identifier | provisioning_status | signed_in_once | completed_at |
|---|---|---|---|---|---|---|---|---|
| `<ref>` | `<name>` | `<work email>` | `<site>` | `<role>` | `<id from the console>` | `complete` | `yes` | `<timestamp>` |

Allowed `provisioning_status` values:

- `complete`: account created, `account_identifier` recorded, `signed_in_once` is `yes`, `completed_at` set.
- `already provisioned`: the person had an individual account before this run. Record the existing
  `account_identifier`. `completed_at` is the date you confirmed it.
- `pending`: not finished. Migration is not complete while any row is `pending`.

Never record a second account for a person who already has one.

## D. Operational record: shared account

- account_status: `disabled` (the status now, not the status at any earlier point)
- delegate: `<name the owner has named in writing, or none>`
- rollbacks_in_run: `<number>` (copy from section 11 of the runbook)

Decisions to disable: one entry per disablement, written in step 7.1 before the shared account is touched.

- decided_by: `<owner name>`
  - decided_at: `<YYYY-MM-DDTHH:MM:SSZ>`

Disablements: one entry per time the shared account was disabled. Copy each from section 11 of the runbook.

- disabled_at: `<YYYY-MM-DDTHH:MM:SSZ, from the console>`
  - disabled_by: `<name>`
  - sessions_ended: `yes | not available`
  - reenabled_at: `<timestamp, only if a rollback re-enabled the account; otherwise blank>`

Verifications: one entry per attempt. Copy each from section 11 of the runbook.

- verification_recorded_at: `<timestamp>`
  - verification_outcome: `rejected | accepted | not performed`

If a rollback was completed in this run, `account_status` must be `enabled` at that point, and the run is not complete
until disablement has been repeated and verified again.

## E. Migration success criteria

The owner ticks each item. All items must be ticked before the migration is marked complete.

- [ ] Section B: the `LEGACY` entry exists, is timestamped, and has a legacy identifier.
- [ ] Section B: the last-known contact and approximate usage are recorded, or `unknown` with a named responsible person.
- [ ] Section C: every intended user has one row.
- [ ] Section C: every row has a `provisioning_status` of `complete` or `already provisioned`, with an `account_identifier`.
- [ ] Section C: no row is `pending`.
- [ ] Section D: `account_status` is `disabled`, and every disablement entry has a `disabled_at`.
- [ ] Section D: the latest verification entry has `verification_outcome` `rejected`. If it is `not performed`, the reason is recorded and the owner has signed off on it.
- [ ] Section D: if there were rollbacks, the final disablement was verified after the last rollback.
- [ ] Nothing in this record is a password, hash, token or session value.

## F. Owner sign-off

- owner_name: `<name>`
- signed_at: `<YYYY-MM-DDTHH:MM:SSZ>`
- statement: `I confirm that the items in section E are true for this run.`

## G. Verification checks

Automated checks or a reviewer can run these against the completed record. Each check has a pass rule.

- **V1 Legacy entry present.** Section B has the label `LEGACY`, a `recorded_at` timestamp and a `legacy_identifier`.
  Pass: all three are non-empty.
- **V2 Legacy fields present.** Section B has `last_known_contact` and `approximate_usage`. Pass: each is either a value
  or `unknown` with `contact_unknown_responsible` set.
- **V3 One row per intended user.** Section C has one row per person on the staff list. Pass: the row count equals the
  intended-user count, and no `staff_ref` is repeated.
- **V4 Provisioning complete.** Each row in section C has `provisioning_status` `complete` or `already provisioned`,
  and a non-empty `account_identifier`. Pass: no row is `pending`, and no row is missing a value.
- **V5 Signed in once.** Each `complete` row has `signed_in_once` `yes` and a `completed_at` timestamp. Pass: all
  true.
- **V6 Shared account disabled.** Section D has `account_status` `disabled` and at least one disablement entry with a
  `disabled_at` timestamp. Pass: all present, and no `disabled_at` is earlier than `started_at`.
- **V7 Verification recorded.** Section D has at least one verification entry, and the latest one has
  `verification_outcome` `rejected`, or `not performed` with a reason and owner sign-off. Pass: one of the two.
- **V8 No secrets.** The record contains no password, hash, token or session value. Pass: a search for those words
  finds only the rule text above.

## H. Outline check job for a repository run

This is a description for a later pipeline. It is not configured in this repository, and no CI file is changed.

1. Read this record and the `legacy-shared-login-acceptance-record.schema.json` file next to it.
2. Run V1 to V8 in order and report each one as pass or fail with the field that failed.
3. Fail the job if any check fails, or if section E has an unticked item.
