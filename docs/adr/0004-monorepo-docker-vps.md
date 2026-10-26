# ADR-0004: Monorepo, Docker Compose, deploy VPS Linux

- **Trạng thái**: Accepted
- **Ngày**: 2026-10-22
- **Người quyết định**: Chủ dự án

## Bối cảnh

Team 1–3 người làm cả backend và frontend. Cần nơi đặt code, cách chạy local thống nhất, và nơi chạy production rẻ. Ứng viên repo: monorepo vs 2 repo. Ứng viên hạ tầng: VPS + Docker, Azure App Service, chỉ local.

## Quyết định

Một repo với `backend/`, `frontend/`, `infra/`, `docs/`. Local: Docker Compose chạy Postgres + Mailpit, backend chạy `dotnet run` ngoài container. Production: một VPS Ubuntu (2 vCPU/4 GB) chạy Docker Compose gồm Nginx, API, Postgres, backup; frontend ưu tiên Vercel. CI/CD bằng GitHub Actions, image đẩy lên GHCR, deploy qua SSH.

## Lựa chọn đã cân nhắc

- **2 repo**: tách CI gọn, nhưng đồng bộ API contract (openapi.json → client TS) qua 2 repo phiền, PR đổi cả API lẫn UI phải tách đôi; team nhỏ không hưởng lợi.
- **Azure App Service + Azure PostgreSQL**: ít vận hành, nhưng chi phí ~30–60 USD/tháng tối thiểu, gấp 3–5 lần VPS; vendor lock-in nhẹ.
- **VPS + Docker**: ~6–12 USD/tháng, kiểm soát hoàn toàn, học được vận hành; phải tự lo bảo mật OS, backup, TLS.

## Hệ quả

Tốt: một PR chứa trọn feature xuyên BE/FE; `docker compose up` là đủ cho người mới; chi phí thấp; quy trình deploy minh bạch (script đọc được).

Đánh đổi: phải tự quản VPS (cập nhật, firewall, backup); downtime vài giây khi recreate container; CI monorepo cần path filter để không chạy thừa.

Việc phải làm theo: `infra/scripts/deploy.sh` có rollback; backup ngoài VPS; checklist VPS hardening trong docs/08; cân nhắc managed Postgres khi doanh thu đủ.
