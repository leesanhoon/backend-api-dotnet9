# Repo Orchestration Bridge

Ap dung cho toan bo repo nay.

## Default behavior

Voi moi task non-trivial, agent phai:
1. Doc cac file orchestration truoc khi implement:
   - `.codex/prompt/init.md`
   - `.codex/prompt/subagent-runtime-bridge.md`
   - `.codex/memory/project-context.md`
   - `.codex/memory/orchestration-memory.md`
   - `.codex/templates/task-routing.md`
2. Dien giai cac ten nhu `code_mapper`, `backend_engineer`, `database_engineer`, `security_reviewer`, `test_validator`, `docs_writer` theo `.codex/prompt/subagent-runtime-bridge.md`:
   - day la role logical cua repo
   - uu tien map sang runtime-supported subagents neu Codex ho tro
   - neu runtime khong spawn duoc hoac khong co dung role name, main agent phai tu thuc hien role do va giu nguyen handoff/report format
3. Khao sat cau truc, file lien quan, route, API, model, service, DbContext, migration, validation, conventions truoc khi code.
4. Lap plan ngan truoc khi sua code.
5. Su dung orchestration 1 main agent + nhom 6 specialist roles khi task vuot qua 1 khu vuc hoac co ambiguity/risk.
6. Chi bat dau code sau khi da co evidence mapping/phat hien tu phase khao sat.
7. Chon thay doi nho nhat hop ly.
8. Validate bang test/build/check phu hop nhat.
9. Sau khi xong, neu co pattern tai su dung duoc, cap nhat `.codex/memory/orchestration-memory.md`.

## Default 6-agent routing

- `code_mapper`: map codebase va conventions.
- `backend_engineer`: controller/service/request/business logic.
- `database_engineer`: entity/DbContext/migration/relationships.
- `security_reviewer`: auth/input validation/upload/file exposure.
- `test_validator`: validation strategy/build/test/regression.
- `docs_writer`: implementation note/doc update khi can.

Tat ca cac ten tren phai duoc hieu la role logical truoc, khong mac dinh la runtime agent type co san.

## Routing rules

- Task trivial, 1 file, risk thap: co the khong can spawn agents.
- Backend/API task: dung toi thieu `code_mapper` + `backend_engineer`.
- Co thay doi schema/data: them `database_engineer`.
- Co upload/file/auth/public asset: them `security_reviewer`.
- Co thay doi hanh vi: them `test_validator`.
- Co thay doi contract/flow quan trong: them `docs_writer`.

## Handoff format cho sub-agents

Main agent giao viec theo format:
- Objective
- Scope
- Constraints
- Files to inspect
- Expected output

Sub-agent tra ve ngan gon:
- Findings
- Risks
- Recommendation
- File references

## Editing rules

- Giu patch nho va trung voi pattern san co.
- Khong sua file khong lien quan.
- Khong them library moi neu chua can thiet ro rang.
- Truoc khi edit, noi ro file nao se doi va vi sao.
- Sau khi edit, tom tat da doi gi va da validate nhu the nao.
