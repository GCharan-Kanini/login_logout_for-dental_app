# Legacy shared-login migration runbook

Replaces one shared clinic login with individual staff accounts, and keeps a record of the legacy login.
Scope: one clinic or one site per run. Story US-005. Sections 1-6 cover legacy capture and mapping (TASK-016);
sections 7-11 cover disablement, verification and rollback (TASK-017).

## 1. Scope and prerequisites

- You are the clinic owner, or act for the owner with their agreement.
- You can create user accounts in the clinic's identity system (administrator console), and you can disable
  accounts there. Placeholders in angle brackets, such as `<admin console>`, are filled in for your system.
- Have the list of staff who need an individual account, with their names and work emails, before you start.
- Record the login identifier only. Never record, copy or export the password, a password hash or a token.

## 2. Legacy-capture checklist

Complete each item, then tick it. Store the values in the acceptance record (see the acceptance-record template).

- [ ] **Legacy identity used**: the shared login identifier (username or email as typed at sign-in).
      Do not record the password.
- [ ] **Last-known contact / email**: the address the shared login sends mail to. If unknown, write `unknown`
      and go to the edge case in section 6.
- [ ] **Approximate usage summary**: which sites and roles use the login, roughly how many people, and at which
      times of day. Estimates are fine; say they are estimates.
- [ ] **Sites or clinics sharing this login**: the list of every site that signs in with it (see section 6).
- [ ] **Existing individual accounts**: staff who already have their own account (see section 6).
- [ ] **Owner sign-off**: the owner's name and the date the capture was done.

## 3. Step sequence

Follow the steps in order. Do not start step 4 until steps 1 to 3 are complete.

1. **Open the legacy record.** Copy the acceptance-record template (`legacy-shared-login-acceptance-record.md`) for this
   clinic and this run. Note the start time.
2. **Capture the legacy identity.** Enter the legacy identifier, the last-known contact and the approximate usage
   summary from section 2. Mark each checklist item.
3. **Confirm the sites and existing accounts.** List every site that uses the login and every staff member who
   already has an individual account.
4. **Prepare provisioning.** For each intended user, fill one row of the mapping template (section 4). Check that
   the target field values are complete.
5. **Create individual accounts manually.** In `<admin console>`, create one account per intended user, using the
   row from the mapping template. Record the account identifier and the time created in the acceptance record.
6. **Check the new accounts.** Each new account signs in and sees the work it needs. Tick the staff provisioning
   checklist in the acceptance record for each user.
7. **Disable the shared account.** Follow the disablement steps in section 7 of this runbook.
8. **Verify the disablement.** Follow the verification step in section 8.
9. **Close the run.** Complete the migration success criteria in the acceptance record.

## 4. Mapping template

For each legacy shared-login attribute, copy the target field into the staff-account form. Then complete the owner
actions for that row.

| Legacy attribute | Target staff-account field | Owner action |
|---|---|---|
| Legacy identity used (shared identifier) | Not copied. Staff get their own identifier. | Do not reuse the shared identifier for any person. |
| Last-known contact / email | Not copied to staff accounts. Recorded only as legacy data. | Keep in the legacy entry. Use the staff member's own work email for their account. |
| Approximate usage summary (site, role) | Role / site assignment on the staff account | Choose the role and site that match the person's work. |
| Staff member full name | Display name | Enter the person's full name as they use it at work. |
| Staff member work email | Sign-in email | Enter the person's own work email. Confirm it with them. |
| Staff member site | Site membership | Assign the site the person works at. |

Owner actions for each intended user:

- **Create individual account**: in `<admin console>`, open the user creation form, enter the display name, work
  email, site and role from the row, then save. Record the account identifier.
- **Record provisioning**: in the acceptance record, set that user's provisioning status to `complete` only after
  they have signed in once.

Owner action to disable the shared account: see section 7 (disablement) of this runbook. Do not disable it until
every intended user has a `complete` provisioning entry.

## 5. Usage note for clinic owners

- Work top to bottom. Each step lists what you need before you start.
- Keep the acceptance record open. Fill it as you go, not from memory at the end.
- The mapping template is one row per person. Duplicate the row for each additional person.
- If you are unsure about a value, write `unknown` and say who will confirm it. Do not guess.

## 6. Edge cases

- **Owner unavailable to provision manually.** Record the planned provisioning in the acceptance record with the
  name of the person who will do it, and leave the run open. Do not disable the shared account until every
  intended user has a `complete` entry.
- **Several clinics or sites use the same shared login.** Make one acceptance record per clinic or site. Disable the
  shared account only after every site's staff have a `complete` entry. Disabling it early cuts off the sites
  still using it.
- **Shared-login identifier is missing or unknown.** Stop before step 4. Do not provision or disable anything until
  the identifier is found, because step 7.2 and check V1 both need it. Ask the person who manages sign-in, or the
  identity-system administrator, to find it, and record who is looking in the acceptance record. Do not invent a value.
- **Last-known contact is missing or unknown.** Record `unknown`, name the person responsible for finding it, and
  record the owner's sign-off on that gap. Do not invent a value.
- **Partial migration: some staff already have individual accounts.** List them in the existing-accounts item in
  section 2. Mark their provisioning entry as `already provisioned` with the account identifier. Do not create a
  second account for them.
- **Risk of removing access by mistake.** Disable the shared account; do not delete it. Deletion is not part of this
  runbook. Rollback is in section 9.
- **Plaintext credentials.** The runbook never asks for a password, hash or token. If one is pasted into the
  acceptance record, remove it at once and tell the owner.

## 7. Disablement

Do these steps in order. Do not start until every intended user has a `complete` (or `already provisioned`) entry
in the acceptance record (section 4), and every site on the sharing list has its staff provisioned (section 6).
Disable the shared account. Never delete it.

Each step lists a **Precondition**, the **Action**, and the **Immediate observable result**. If the result you see is
not the one listed, stop and go to section 10.

### 7.1 Record the disablement decision

- **Precondition**: the checks above are complete.
- **Action**: in section D of the acceptance record, add a decision entry with `decided_by` (the owner's name) and
  `decided_at` (the current time).
- **Immediate observable result**: the decision entry shows `decided_by` and `decided_at`.

### 7.2 Confirm you have the right account

- **Precondition**: 7.1 is recorded.
- **Action**: in `<admin console>`, search for the legacy identifier recorded in section 2 and open the account.
  Compare its identifier with the legacy record character by character.
- **Immediate observable result**: the account identifier matches the legacy record exactly. If it does not, stop.

### 7.3 Disable the shared account

- **Precondition**: 7.2 matched.
- **Action**: in `<admin console>`, use the **disable** action on the shared account. Save. Do not delete the account,
  do not change its password, and do not change its identifier.
- **Immediate observable result**: the account status reads `disabled`, and the console shows a disabled time.
  Copy that time as `disabled_at` into the run metadata (section 11).

### 7.4 End active sessions for the shared account

- **Precondition**: 7.3 shows `disabled`.
- **Action**: in `<admin console>`, open the account's sessions and end any that are listed. If the console has no
  sessions view for this account, write `not available` in the run metadata.
- **Immediate observable result**: the sessions list for the shared account is empty, or the run metadata says
  `not available`.

### 7.5 Update the operational record

- **Precondition**: 7.3 and 7.4 are complete.
- **Action**: in the acceptance record, set the shared account's operational status to `disabled` and enter
  `disabled_at`.
- **Immediate observable result**: the operational record reads `disabled` with a timestamp.

## 8. Verification

Run this once, straight after section 7. The owner types the shared credentials. They are not copied from any
record.

- **Precondition**: section 7 is complete, and the owner has the shared password to hand.
- **Action**: on the clinic sign-in page, enter the legacy identifier and the shared password, and submit **once**.
  Do not retry. Repeated failures can trigger a lockout that affects other users.
- **Expected result**: the sign-in is **rejected with an authentication failure**. The page shows an error, and no
  session is created. Record the exact error text or status code the system shows. This runbook does not assume a
  particular code.
- **Record the outcome**: in section 11, add an entry under `verification` with `outcome` set to `rejected`, and enter
  the time, the person and the observed response. Never overwrite an earlier entry.

If the sign-in **succeeds**, verification has failed. Do not tell staff to use the shared login. Record the outcome as
`accepted`, then check the shared account's status in `<admin console>`. If it reads `enabled`, repeat 7.3. If it
reads `disabled` but the sign-in still works, stop and follow the escalation in section 10.

## 9. Rollback

Rollback restores access through the shared login. It does not delete or change any individual staff account.

**Who may trigger it**: the clinic owner, or a delegate the owner names in writing in the run metadata.

**When to trigger it**: staff cannot do their work and no individual account fix is possible that day, a site still
needs the shared login, or section 10 says to roll back.

### 9.1 Record the reason

- **Precondition**: the owner has decided to roll back.
- **Action**: in section 11, add an entry under `rollbacks` with the request time, who requested it, and the reason.
- **Immediate observable result**: the `rollbacks` entry exists with a time and a reason.

### 9.2 Re-enable the shared account

- **Precondition**: 9.1 is recorded.
- **Action**: in `<admin console>`, open the **same** shared account and use the **enable** action. Do not create a new
  account with the same identifier. That would be a different account.
- **Immediate observable result**: the account status reads `enabled`.

### 9.3 Check that the shared login works

- **Precondition**: 9.2 shows `enabled`.
- **Action**: on the clinic sign-in page, sign in once with the shared identifier and password, typed by the owner.
  Then sign out.
- **Immediate observable result**: the sign-in succeeds and a session is created. Record `login_check_outcome: accepted`.
  If it fails, stop and follow section 10.

### 9.4 Record rollback completion

- **Precondition**: 9.3 succeeded.
- **Action**: in section 11, complete the rollback entry with `completed_at`, `login_check_outcome`, and set the
  operational record back to `enabled`. Set `reenabled_at` on the open `disabled` entry in section 11 (the one whose
  `reenabled_at` is blank). Leave every earlier `disabled_at` entry as it is, and add a new entry if the account is
  disabled again.
- **Immediate observable result**: the rollback entry is complete, and the operational record reads `enabled`.

## 10. Edge-case decisions

For each case: how to detect it, what to do, and when to stop and roll back.

- **Missing credentials.** *Detect*: the owner cannot recall the shared password, so section 8 cannot be attempted.
  *Do*: do not guess, reset or create a password as part of this runbook. Confirm in `<admin console>` that the
  account reads `disabled`, and add a `verification` entry with `outcome: not performed`, the reason and the console state.
  Ask the identity-system administrator to confirm the sign-in is blocked. *Checkpoint*: if any site still uses the
  login, do not disable it. If none does, the owner decides whether to proceed, and records that decision.
- **Multi-site use.** *Detect*: section 2 lists more than one site, or a site reports sign-in failures after
  disablement. *Do*: keep the shared account enabled until every site's staff have a `complete` entry. If a site
  cannot sign in after disablement, roll back (section 9). *Checkpoint*: any site still on the shared login blocks
  disablement. A site failure after disablement triggers rollback.
- **Accidental loss of all individual access.** *Detect*: staff cannot sign in with their individual accounts after
  provisioning or disablement. *Do*: first check whether the cause is an individual account (wrong site, role or
  email, or an account not yet active). Fix that. If disablement is the cause, roll back at once (section 9). Never
  create a new account under the shared identifier to get back in. *Checkpoint*: if rollback 9.3 also fails and the
  owner cannot reach `<admin console>`, use the identity-system administrator's recovery route and record the
  incident in section 11.

## 11. Run metadata

Fill in this block for every run. Leave `owner_review` blank until the clinic owner has reviewed the runbook.

```
run_id:
clinic:
site(s):
legacy_record_ref:
delegate: none | <name the owner has named in writing>
disabled:
  - disabled_at:
    disabled_by:
    sessions_ended: yes | not available
    reenabled_at: (blank until a rollback re-enables the account)
verification:
  - attempted_at:
    performed_by:
    outcome: rejected | accepted | not performed
    reason_if_not_performed:
    observed_response:
rollbacks:
  - requested_at:
    requested_by:
    reason:
    completed_at:
    login_check_outcome: accepted | rejected
    operational_record_status: enabled
incidents:
  - time:
    summary:
    action_taken:
owner_review:
  reviewer:
  reviewed_at:
  runbook_version_reviewed:
```
