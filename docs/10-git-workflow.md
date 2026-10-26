# 10 — Git workflow

Team 1–3 người, một repo (monorepo). Quy trình **nhẹ nhưng không bỏ bước review**: mọi thay đổi vào `main` qua PR, CI xanh.

## 1. Branch

| Branch | Vai trò | Ai push |
|---|---|---|
| `main` | Luôn deploy được. Tag `vX.Y.Z` từ đây để deploy prod | Chỉ qua PR merge |
| `develop` | Tích hợp, auto deploy staging | Chỉ qua PR merge |
| `feature/<ticket>-<mo-ta>` | Một feature / task | Người làm |
| `fix/<ticket>-<mo-ta>` | Sửa bug | |
| `hotfix/<mo-ta>` | Sửa khẩn trên prod, nhánh từ `main`, merge về cả `main` và `develop` | |
| `chore/<mo-ta>` | Cấu hình, CI, docs | |

Khi team = 1 người có thể bỏ `develop`, làm feature → PR → `main`. Bật `develop` khi có ≥ 2 người hoặc có staging.

Tên branch: lowercase, gạch ngang, không dấu: `feature/12-place-order`, `fix/45-stock-negative`.

Branch sống **ngắn** (≤ 3 ngày). Feature lớn → tách PR nhỏ, có thể ẩn sau feature flag hoặc chưa map endpoint.

## 2. Commit — Conventional Commits

```
<type>(<scope>): <mô tả ngắn, tiếng Anh, imperative, ≤ 72 ký tự>

[body: tại sao, không phải cái gì]

[footer: Closes #12, BREAKING CHANGE: ...]
```

| `type` | Dùng khi |
|---|---|
| `feat` | Thêm hành vi người dùng thấy được |
| `fix` | Sửa bug |
| `refactor` | Đổi code không đổi hành vi |
| `test` | Chỉ thêm/sửa test |
| `docs` | Tài liệu |
| `chore` | Build, CI, deps, config |
| `perf` | Hiệu năng |
| `db` | Migration (dùng kèm `feat` nếu có cả logic) |

`scope` = module hoặc layer: `orders`, `catalog`, `inventory`, `auth`, `api`, `infra`, `web`, `docs`.

Ví dụ:

```
feat(orders): reserve stock when placing order

Reservation prevents overselling between place and confirm.
Released on cancel, committed on confirm (see docs/03 §4).

Closes #23
```

Quy tắc:
- Mỗi commit build được và test xanh (để `git bisect` dùng được).
- Không commit `WIP`, `fix typo` lên `main` → squash khi merge.
- Không commit file sinh ra (`bin/`, `obj/`, `node_modules/`, `.env`). `.gitignore` đã có.
- Commit migration **cùng** commit đổi entity.

## 3. Pull Request

- Tiêu đề = commit message đầu tiên theo Conventional Commits (vì squash merge dùng tiêu đề PR).
- Template `.github/PULL_REQUEST_TEMPLATE.md`:

```markdown
## Mục đích
<!-- 1-3 câu: thay đổi gì, vì sao. Link issue. -->

## Thay đổi chính
- 

## Checklist
- [ ] Test thêm/sửa, `dotnet test` xanh local
- [ ] Không có warning build
- [ ] Migration (nếu có) đã review file .cs, không sửa migration cũ
- [ ] openapi.json / frontend client regenerate (nếu đổi API)
- [ ] docs/ cập nhật (nếu đổi nghiệp vụ hoặc kiến trúc), ADR (nếu đổi quyết định)
- [ ] Không log / commit dữ liệu nhạy cảm

## Cách kiểm tra
<!-- bước thủ công nếu có -->

## Screenshot (UI)
```

- Kích cỡ: **≤ 400 dòng diff** (không tính migration snapshot, lock file, test data). Lớn hơn → tách.
- Merge: **Squash and merge** vào `develop`/`main`. Xóa branch sau merge.
- Yêu cầu: CI xanh; ≥ 1 approve nếu team ≥ 2; tự merge được nếu solo nhưng vẫn mở PR để CI chạy và có lịch sử.
- Người tạo PR tự review diff của mình trước khi xin review.

## 4. Code review — tiêu chí

Reviewer kiểm theo thứ tự:

1. **Đúng nghiệp vụ** theo `docs/03` không? State machine, invariant.
2. **Đúng chỗ** theo `docs/02` không? Logic trong Domain, không trong endpoint/handler.
3. **An toàn**: input validate, quyền kiểm tra, không lộ dữ liệu, không N+1.
4. **Test** đủ nhánh chính và lỗi.
5. **Naming & convention** theo `docs/04`.
6. Cuối cùng mới đến style cá nhân (và chỉ góp ý `nit:`).

Dùng tiền tố comment: `blocker:` (phải sửa), `suggestion:`, `question:`, `nit:`. Tác giả giải quyết mọi `blocker`, trả lời mọi `question`.

## 5. Versioning và release

- SemVer cho API/backend: `vMAJOR.MINOR.PATCH`.
  - `MAJOR`: breaking API (`/api/v2`).
  - `MINOR`: feature mới.
  - `PATCH`: fix.
- Release = tag trên `main`: `git tag -a v0.3.0 -m "..." && git push --tags` → CD deploy prod.
- `CHANGELOG.md` sinh từ commit (`git-cliff` hoặc GitHub Release notes tự động). Không viết tay.
- MVP ra mắt = `v1.0.0`. Trước đó `v0.x`.

## 6. Issue / task tracking

- GitHub Issues + Projects (board Kanban: Backlog → Todo → In progress → Review → Done).
- Label: `module:*`, `type:bug|feature|chore`, `priority:p0..p2`, `good-first-issue`.
- Mỗi task trong [12-roadmap.md](12-roadmap.md) tương ứng 1 issue khi bắt đầu phase.
- Issue mô tả **hành vi mong đợi** và **tiêu chí chấp nhận**, không mô tả cách code.

## 7. Thiết lập ban đầu

```bash
git init
git branch -M main
git add .
git commit -m "chore: bootstrap repository with architecture docs"
# tạo repo GitHub, private
git remote add origin git@github.com:<org>/snackshop.git
git push -u origin main
```

Bảo vệ `main` trên GitHub: Require PR, Require status checks (`ci / backend`, `ci / frontend`), Require linear history, Do not allow bypass.

Cài `pre-commit` hook (qua `husky` ở frontend hoặc script `.githooks/pre-commit`): `dotnet format --verify-no-changes` cho file C# staged, `pnpm lint-staged` cho frontend. Không bắt buộc nhưng tiết kiệm vòng CI.
