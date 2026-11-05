---
name: SnackShop
description: Bao bì snack Việt - mặt trước gói snack in offset phóng to thành trang web; kệ kem, bốn màu bao bì, dải sọc chéo, ngôi sao khuyến mãi, con tem giá tròn.
colors:
  chili: "oklch(0.67 0.21 32)"
  chili-deep: "oklch(0.5 0.19 29)"
  chili-shadow: "oklch(0.4 0.15 30)"
  chili-tint: "oklch(0.95 0.04 35)"
  mango: "oklch(0.86 0.17 88)"
  mango-deep: "oklch(0.78 0.17 85)"
  mango-text: "oklch(0.42 0.09 80)"
  mango-tint: "oklch(0.96 0.07 92)"
  pandan: "oklch(0.62 0.15 152)"
  pandan-deep: "oklch(0.5 0.14 152)"
  pandan-tint: "oklch(0.95 0.05 152)"
  cobalt: "oklch(0.5 0.21 264)"
  cobalt-deep: "oklch(0.42 0.19 264)"
  cobalt-tint: "oklch(0.94 0.04 264)"
  cream: "oklch(0.98 0.02 85)"
  cream-2: "oklch(0.95 0.03 85)"
  paper: "oklch(1 0 0)"
  ink: "oklch(0.22 0.012 60)"
  ink-soft: "oklch(0.48 0.03 60)"
  rule: "oklch(0.9 0.035 80)"
  input: "oklch(0.8 0.04 75)"
  on-colour: "oklch(0.995 0 0)"
typography:
  display:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "4.5rem"
    fontWeight: 800
    lineHeight: 0.95
  headline:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "3rem"
    fontWeight: 800
    lineHeight: 1
  headline-page:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1.875rem"
    fontWeight: 800
    lineHeight: 1
  title:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 800
    lineHeight: 1.25
  title-sm:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 800
    lineHeight: 1.25
  figure:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1.875rem"
    fontWeight: 800
    lineHeight: 1
    fontVariation: "tabular-nums"
  price:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 800
    lineHeight: 1
    fontVariation: "tabular-nums"
  body:
    fontFamily: "Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
  body-sm:
    fontFamily: "Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.43
  button:
    fontFamily: "Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 700
    lineHeight: 1.43
  caption:
    fontFamily: "Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 400
    lineHeight: 1.33
  label:
    fontFamily: "Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "0.6875rem"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "0.05em"
  seal:
    fontFamily: "Baloo 2, Be Vietnam Pro, system-ui, sans-serif"
    fontSize: "0.6rem"
    fontWeight: 800
    lineHeight: 1.05
    letterSpacing: "0.02em"
rounded:
  sm: "6px"
  md: "8px"
  lg: "10px"
  xl: "12px"
  2xl: "16px"
  3xl: "28px"
  full: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  2xl: "32px"
  3xl: "48px"
  bag-pad: "16px"
  bag-face-pad: "40px"
  seal-strip: "16px"
components:
  button-primary:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    typography: "{typography.button}"
    rounded: "{rounded.full}"
    padding: "8px 20px"
    height: "40px"
  button-primary-hover:
    backgroundColor: "{colors.chili-deep}"
  button-secondary:
    backgroundColor: "{colors.mango}"
    textColor: "{colors.ink}"
    typography: "{typography.button}"
    rounded: "{rounded.full}"
    padding: "8px 20px"
    height: "40px"
  button-secondary-hover:
    backgroundColor: "{colors.mango-deep}"
  button-outline:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.full}"
    padding: "8px 20px"
    height: "40px"
  button-outline-hover:
    backgroundColor: "{colors.cream-2}"
  button-ghost:
    textColor: "{colors.ink}"
    rounded: "{rounded.full}"
    padding: "8px 20px"
    height: "40px"
  button-ghost-hover:
    backgroundColor: "{colors.cream-2}"
    textColor: "{colors.chili-deep}"
  button-destructive:
    backgroundColor: "{colors.chili-deep}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "8px 20px"
    height: "40px"
  button-link:
    textColor: "{colors.chili-deep}"
    padding: "0"
  button-sm:
    rounded: "{rounded.full}"
    padding: "0 14px"
    height: "32px"
  button-lg:
    rounded: "{rounded.full}"
    padding: "0 28px"
    height: "48px"
  badge-sticker:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    typography: "{typography.caption}"
    rounded: "{rounded.md}"
    padding: "2px 8px"
  badge-sticker-warning:
    backgroundColor: "{colors.mango-tint}"
    textColor: "{colors.mango-text}"
    rounded: "{rounded.md}"
    padding: "2px 8px"
  badge-sticker-success:
    backgroundColor: "{colors.pandan-tint}"
    textColor: "{colors.pandan-deep}"
    rounded: "{rounded.md}"
    padding: "2px 8px"
  badge-sticker-info:
    backgroundColor: "{colors.cobalt-tint}"
    textColor: "{colors.cobalt-deep}"
    rounded: "{rounded.md}"
    padding: "2px 8px"
  badge-sticker-destructive:
    backgroundColor: "{colors.chili-tint}"
    textColor: "{colors.chili-deep}"
    rounded: "{rounded.md}"
    padding: "2px 8px"
  badge-stamp:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    typography: "{typography.label}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  badge-stamp-warning:
    backgroundColor: "{colors.mango}"
    textColor: "{colors.ink}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  badge-stamp-success:
    backgroundColor: "{colors.pandan}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  badge-stamp-info:
    backgroundColor: "{colors.cobalt}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  badge-stamp-destructive:
    backgroundColor: "{colors.chili-deep}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  badge-stamp-muted:
    backgroundColor: "{colors.cream-2}"
    textColor: "{colors.ink-soft}"
    rounded: "{rounded.full}"
    padding: "4px 10px"
  card:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    rounded: "{rounded.2xl}"
    padding: "16px"
  card-desktop:
    padding: "24px"
  bag:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.3xl}"
    padding: "28px 16px 16px"
  bag-face:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.3xl}"
    padding: "48px 40px 40px"
  price-stamp:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    typography: "{typography.price}"
    rounded: "{rounded.full}"
    padding: "4px"
    size: "80px"
  burst:
    backgroundColor: "{colors.mango}"
    textColor: "{colors.ink}"
    typography: "{typography.title-sm}"
    padding: "20px"
    size: "176px"
  seal:
    backgroundColor: "{colors.mango}"
    textColor: "{colors.ink}"
    typography: "{typography.seal}"
    padding: "6px"
    size: "56px"
  step-disc:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    size: "28px"
  input:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    typography: "{typography.body-sm}"
    rounded: "{rounded.lg}"
    padding: "8px 12px"
    height: "40px"
  search:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    typography: "{typography.body-sm}"
    rounded: "{rounded.full}"
    padding: "0 16px 0 40px"
    height: "44px"
  category-pill:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.ink}"
    typography: "{typography.button}"
    rounded: "{rounded.full}"
    padding: "0 16px"
    height: "36px"
  category-pill-active:
    backgroundColor: "{colors.pandan}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "0 16px"
    height: "36px"
  nav-item:
    textColor: "{colors.ink}"
    rounded: "{rounded.full}"
    padding: "8px 16px"
  nav-item-active:
    backgroundColor: "{colors.chili}"
    textColor: "{colors.on-colour}"
    rounded: "{rounded.full}"
    padding: "8px 16px"
  stat-tile:
    backgroundColor: "{colors.paper}"
    textColor: "{colors.chili-deep}"
    typography: "{typography.figure}"
    rounded: "{rounded.2xl}"
    padding: "16px"
  stat-tile-warning:
    backgroundColor: "{colors.mango}"
    textColor: "{colors.ink}"
    rounded: "{rounded.2xl}"
    padding: "16px"
  table-head:
    backgroundColor: "{colors.cream-2}"
    textColor: "{colors.ink-soft}"
    typography: "{typography.label}"
    padding: "0 12px"
    height: "40px"
  table-cell:
    typography: "{typography.body-sm}"
    padding: "10px 12px"
  dialog:
    backgroundColor: "{colors.paper}"
    rounded: "{rounded.2xl}"
    padding: "24px"
    width: "512px"
---

# Design System: SnackShop

Tài liệu này ghi lại hệ thống hình ảnh **như đã được xây** trong `frontend/` (Next.js 16, Tailwind CSS v4, Radix, lucide-react). Token trong frontmatter là chuẩn; phần văn xuôi giải thích dùng ở đâu và vì sao. Nguồn token: `frontend/src/app/globals.css` (khối `:root`, `@theme inline`, các `@utility`), font: `frontend/src/app/layout.tsx`, bảng màu bao bì: `frontend/src/lib/tone.ts`. Sidecar `.impeccable/design.json` giữ phần schema không chứa được (dải tông màu, bóng, motion, breakpoint, snippet component). Thế giới này thay thế hoàn toàn thế giới "tập ô ly" đã bị từ chối ngày 2026-11-05; không token nào của bản cũ còn tồn tại.

## Overview

**Creative North Star: "Bao bì snack Việt"**

Toàn bộ giao diện là mặt trước một gói snack in offset phóng to thành trang web. Nền kem là kệ hàng; mỗi danh mục và mỗi sản phẩm là một gói snack in bằng **một** trong bốn màu bao bì (ớt, xoài, lá dứa, cô-ban), dùng cả mảng chứ không chấm phá. Mép trên của mỗi gói là dải sọc chéo 45° đỏ-vàng được "ép nhiệt" răng cưa; dải sọc ấy cũng là băng thương hiệu chạy hết chiều ngang header. Giá là con tem tròn trắng viền màu dán ở góc gói; khuyến mãi duy nhất trên kệ là một ngôi sao vàng; tên tiệm và tiêu đề in bằng Baloo 2 nét dày với bóng lệch cứng như in chồng hai lượt mực.

Hệ thống từ chối hai khuôn mẫu: trang marketplace trắng (banner trên một hàng thẻ đều nhau với nút cam) và thế giới vở học sinh trước đó (giấy kẻ ô, lề đỏ, con dấu nghiêng, danh sách đánh số, chữ viết tay). Tông màu **sáng, chỉ sáng**: kệ hàng dưới đèn huỳnh quang không có chế độ tối. Ảnh sản phẩm luôn là hình đĩa tròn viền trắng mờ dán lên mặt gói; chữ trên màu luôn trắng (hoặc mực đậm trên vàng xoài).

Admin là quầy sau kệ: nền kem, dải sọc mỏng chỉ ở mép trên cùng, sidebar là hộp giấy trắng; màu chỉ dành cho mục đang chọn, chip trạng thái, ô số cần xử lý (vàng xoài) và hành động chính. Chuyển động được viết đúng hai lần: gói hàng **nhấc** khỏi kệ khi hover (dịch lên 3px, xoay -0,6°, bóng mềm), và viên giỏ hàng **nảy** một lần khi có món mới; mọi thứ khác chỉ đổi màu trong 150ms.

**Key Characteristics:**
- Kệ kem (`cream`) làm nền; hộp giấy trắng viền mảnh cho nội dung "in sẵn"; gói màu bo 28px cho danh mục, sản phẩm và mặt gói chi tiết.
- Bốn màu bao bì, mỗi màu một việc: ớt là thương hiệu/hành động chính, xoài là CTA trên màu và cảnh báo, lá dứa là thành công/còn hàng, cô-ban là thông tin. Đồ uống (danh mục thứ 5) là gói trắng viền ớt 3px.
- Dải sọc chéo 45° ớt-xoài là băng thương hiệu (header 72px, footer, dải admin 10px) và mép ép nhiệt răng cưa 16–20px trên mỗi gói.
- Baloo 2 800 cho mọi tiêu đề, giá, con số; bóng lệch cứng (`overprint`) chỉ cho tên tiệm và tiêu đề trên mặt gói. Be Vietnam Pro cho thân bài và điều khiển.
- Điều khiển là viên thuốc (`rounded-full`): nút, pill danh mục, mục nav, phân loại, tem trạng thái. Trên nền màu, viên là trắng; đang chọn thì tô đặc màu.
- Giá là con tem tròn trắng viền 3px màu của gói; ưu đãi là ngôi sao 24 cánh vàng xoài.
- Bóng chỉ khi gói được nhấc (hover) hoặc nổi khỏi trang (dialog, drawer); mọi thứ khác phẳng.

## Colors

Bảng màu là mực in offset trên giấy bạc: bốn màu bao bì bão hòa, mỗi màu có bản đậm và bản nhuộm nhạt, đặt trên kệ kem ấm. Tất cả token khai báo bằng OKLCH trong `globals.css`; không chuyển sang hex.

### Primary
- **Ớt** (`chili`): màu thương hiệu và hành động chính trên nền kem: nút "Đặt hàng", "Tiến hành thanh toán", mục nav đang chọn (admin, tài khoản), trang hiện tại trong phân trang, đĩa số bước, thanh cuộn, caret, màu theme trình duyệt. Là gói của danh mục đầu tiên (khô gà & khô bò), mặt gói hero và nền footer. Dùng cho `--primary`, `--ring`, `--chart-1`.
- **Ớt đậm** (`chili-deep`): hover của nút chính, chữ giá và tổng tiền trên nền kem/trắng (`PriceTag`, tổng đơn, con số `StatTile`, doanh thu), liên kết (`link` button, breadcrumb hover), chữ của gói Đồ uống, nút và tem `destructive`. Dùng cho `--destructive` và `--accent-foreground`.
- **Bóng ớt** (`chili-shadow`): màu duy nhất của bóng chữ `overprint` (3px/2px lệch), và màn phủ 40% + blur 2px sau dialog/drawer.
- **Ớt nhuộm** (`chili-tint`): nền sticker và Alert lỗi, nền `ErrorState`, nền ô phương thức thanh toán đang chọn. Dùng cho `--accent`.

### Secondary
- **Xoài** (`mango`): CTA đặt trên gói màu ("Xem thực đơn", "Mua ngay", viên giỏ hàng trong header), nửa sáng của dải sọc, ngôi sao freeship, con dấu "Bán chạy", viền ô tìm kiếm khi focus, ô `StatTile` cảnh báo, pill "Freeship từ…" ở footer, màu bôi chọn văn bản. Tem trạng thái `warning` (Chờ xác nhận, Chưa thanh toán) và `secondary` (Đang giao). Dùng cho `--secondary`, `--warning`, `--chart-2`.
- **Xoài đậm** (`mango-deep`): hover của nút secondary; viền tem giá trên gói xoài.
- **Chữ trên xoài nhạt** (`mango-text`): chữ của sticker `warning` trên nền `mango-tint`. Dùng cho `--warning-foreground`.
- **Xoài nhuộm** (`mango-tint`): nền sticker cảnh báo (số tồn kho sắp hết).

### Tertiary
- **Lá dứa** (`pandan`): gói danh mục hạt & trái cây sấy, tem và Alert thành công (Đã giao, Đã thanh toán), pill danh mục "Tất cả" khi đang chọn. Dùng cho `--success`, `--chart-3`.
- **Lá dứa đậm** (`pandan-deep`): chữ của sticker/Alert thành công; viền tem giá trên gói lá dứa. Dùng cho `--success-foreground`.
- **Lá dứa nhuộm** (`pandan-tint`): nền sticker và Alert thành công.
- **Cô-ban** (`cobalt`): gói danh mục kẹo & bánh ngọt; tem trạng thái `info` (Đã xác nhận, Đang chuẩn bị). Dùng cho `--info`, `--chart-4`.
- **Cô-ban đậm** (`cobalt-deep`) / **Cô-ban nhuộm** (`cobalt-tint`): chữ và nền của sticker/Alert thông tin (ghi chú đặt hàng không cần tài khoản, email chưa xác nhận).

### Neutral
- **Kem** (`cream`): kệ hàng, nền toàn trang (`--background`), nền hàng pill danh mục và header admin ở 85–95% với `backdrop-blur`.
- **Kem đậm** (`cream-2`): nền hover của nút ghost/outline và hàng bảng, đầu bảng ở 70%, skeleton, nền nhãn "Quầy", đĩa số bước đã qua, tem `muted`, ô nhập vô hiệu, rãnh thanh cuộn. Dùng cho `--muted`.
- **Giấy** (`paper`): hộp giấy trắng: thẻ, bảng, dialog, sidebar admin, ô nhập, ô tìm kiếm, pill trên gói màu, đĩa tem giá, đĩa ảnh, gói Đồ uống. Dùng cho `--card`, `--popover`.
- **Mực** (`ink`): chữ thân bài và tiêu đề trên kem/giấy/xoài; nâu rất đậm chứ không đen tuyệt đối. Dùng cho `--foreground`, `--secondary-foreground`.
- **Mực nhạt** (`ink-soft`): mô tả, phụ đề, giá gốc gạch ngang, đầu cột bảng, số thứ tự bảng, nhãn trục biểu đồ, icon tìm kiếm. Dùng cho `--muted-foreground`.
- **Đường kẻ** (`rule`): viền mặc định của mọi phần tử (`--border`): thẻ, bảng, hàng bảng, hàng dòng đơn, đường dưới hàng pill.
- **Viền ô nhập** (`input`): viền ô nhập ở 70% khi nghỉ, 100% khi hover, chuyển sang ớt khi focus.
- **Chữ trên màu** (`on-colour`): chữ trắng trên ớt, lá dứa, cô-ban (`--primary-foreground`, `--destructive-foreground`). Trên xoài, chữ là `ink`.

### Named Rules
**The Whole-Field Rule.** Màu bao bì là mảng, không phải điểm nhấn. Một gói nhận đúng một màu phủ toàn bộ mặt; màu chọn theo vị trí danh mục (`toneAt`) hoặc hash ổn định của sản phẩm (`toneFor`) để cùng một gói luôn cùng màu. Không dùng màu bao bì làm viền, gạch chân hay icon lẻ trên nền kem.

**The Chili-on-Cream, Mango-on-Colour Rule.** Trên nền kem hoặc giấy trắng, hành động chính là ớt chữ trắng. Trên một gói màu, hành động chính là xoài chữ mực (trên gói xoài thì ngược lại: ớt chữ trắng). Không bao giờ đặt nút ớt lên gói ớt.

**The Light Shelf Rule.** Hệ thống chỉ có một chế độ sáng. Không thêm biến thể tối cho token; không dùng `dark:`.

## Typography

**Display Font:** Baloo 2 (`--font-baloo`, weight 700/800, subset latin + vietnamese; fallback Be Vietnam Pro)
**Body Font:** Be Vietnam Pro (`--font-be-vietnam`, weight 400/500/600/700, subset latin + vietnamese; fallback ui-sans-serif, system-ui)

**Character:** Baloo 2 là chữ in dày, tròn trịa của mặt gói snack, đọc được tiếng Việt có dấu ở cỡ lớn; Be Vietnam Pro là chữ in gọn, trung tính của mặt sau gói (thành phần, hướng dẫn). Cặp đôi tách bạch "in lớn trên mặt gói" (tên tiệm, tiêu đề, giá, con số, nhãn ép) với "in nhỏ phía sau" (thân bài, nút, ô nhập, bảng). Cả trang bật `tabular-nums` nên giá và số thẳng cột. Không có mặt chữ viết tay.

### Hierarchy
- **Display** (Baloo 2 800, 48px điện thoại / 60px từ 640px / 72px từ 1024px, line-height 0,95, `overprint` 3px): tiêu đề hero "Tối nay ăn vặt gì?" trên mặt gói ớt. Một lần, chỉ ở trang chủ.
- **Headline** (Baloo 2 800, 36px / 48px từ 640px, line-height 1, `overprint-sm` 2px): `h1` trên mặt gói chi tiết sản phẩm và nhãn kệ danh mục.
- **Headline-page** (Baloo 2 800, 30px, line-height 1, không bóng): `h1` của mọi trang trên nền kem (Giỏ hàng, Thanh toán, Đơn hàng {mã}, PageHeader admin) và `h2` các section trang chủ ("Món bán chạy", "Hàng mới về"). Tên tiệm trong header: 30px / 36px với `overprint`.
- **Title** (Baloo 2 800, 24px): tên danh mục trong gói danh mục (20px trên điện thoại), tên tiệm trong admin, tên khách ở trang tài khoản, "Mô tả sản phẩm".
- **Title-sm** (Baloo 2 800, 20px, line-height 1,25): `CardTitle`, `DialogTitle`, tiêu đề `EmptyState`, "Thông tin chuyển khoản", tiêu đề cột footer.
- **Figure** (Baloo 2 800, 30px, tabular): con số trong `StatTile`. Biến thể: doanh thu hôm nay 48px, tổng đơn 24px, dòng giỏ hàng 18px, dòng sản phẩm trong đơn 16px (700).
- **Price** (Baloo 2 800, 20px, tabular): `PriceTag` nhỏ và chữ trong tem giá (14px trên kệ, 18px trên mặt gói, 11px trên điện thoại); `PriceTag` lớn 36px. Phần trăm giảm 11px 700 ớt đậm.
- **Body** (Be Vietnam Pro 400, 16px, 1,5): mô tả dài, địa chỉ, đoạn giới thiệu hero (18px từ 640px); giới hạn `max-w-prose`.
- **Body-sm** (Be Vietnam Pro 400–600, 14px): mặc định cho UI: ô nhập, ô bảng, nav (600), tên sản phẩm trong giỏ (600), nhãn form (500), tiêu đề ô số (600).
- **Button** (Be Vietnam Pro 700, 14px; 12px ở `sm`, 16px ở `lg`): nút, pill danh mục, pill "Chọn mua" (800, 12px), phân loại.
- **Caption** (Be Vietnam Pro 400–600, 12px): mô tả ngắn trên gói, thời gian, "Đã bán", số lượng sản phẩm trong gói danh mục (600), sticker (700).
- **Label** (Be Vietnam Pro 800, 11px, tracking rộng, IN HOA): đầu cột bảng admin, tem trạng thái, nhãn "Quầy" (10px).
- **Seal** (Baloo 2 800, 8–9,6px, IN HOA, tracking 0,02em): chữ trong con dấu ngôi sao nhỏ "Bán chạy"/"Hết hàng".

### Named Rules
**The Printed-Face Rule.** Baloo 2 chỉ ở những chỗ được in lớn trên mặt gói: tiêu đề `h1`–`h2`, tên danh mục/tiệm, giá, con số, chữ trong ngôi sao và con dấu, số bước. Không bao giờ ở thân bài, nút, ô nhập, ô bảng, nhãn form. Tên sản phẩm trong gói trên kệ là `h3` Be Vietnam Pro 700 16–18px vì nó là nhãn thành phần, không phải tên nhãn hiệu.

**The Overprint Rule.** Bóng chữ lệch cứng (`overprint` 3px, `overprint-sm` 2px, màu `chili-shadow`) chỉ xuất hiện trên tên tiệm và tiêu đề đặt **trên gói màu** (hero, mặt gói sản phẩm, nhãn kệ danh mục, footer). Tiêu đề trên nền kem hoặc giấy trắng không có bóng.

**The Tabular Rule.** Mọi số tiền và số đếm dùng `tabular-nums` (đã bật ở `body`); định dạng VND không thập phân, ký hiệu "₫" sau số; ngôi sao viết tắt "300K"/"1,2TR".

## Layout

Storefront là một cột rộng tối đa 1152px (`max-w-6xl`) căn giữa trên kệ kem, gutter 20px trên điện thoại và 24px từ 640px, đệm dọc 24/32px. Header là băng sọc 72px (`stripes`) chứa tên tiệm, ô tìm kiếm trắng bo tròn (ẩn trên điện thoại, chuyển xuống dòng riêng dưới băng), nút tài khoản ghost trắng và viên giỏ hàng xoài; dưới băng là hàng pill danh mục cao 36px trên nền kem 85–95% mờ, cuộn ngang trên điện thoại, với "Tra cứu đơn hàng" đẩy sang phải. Header chỉ dính (sticky) từ 768px. Footer là mặt sau gói: dải sọc mỏng 12px rồi mảng ớt ba cột.

Viewport đầu tiên của trang chủ: lưới `3fr 2fr` từ 1024px (gap 24px), trái là mặt gói hero cao tối thiểu 440/520px với ngôi sao ở góc trên phải (144/176px), tiêu đề + mô tả + CTA xếp dưới cùng, đĩa ảnh sản phẩm nổi bật 224/288px lòi ra khỏi góc dưới phải; phải là 5 gói danh mục xếp dọc (cao tối thiểu 84px, gap 12px). Trên điện thoại hai cột xếp chồng, đĩa ảnh ẩn.

Lưới sản phẩm (`ProductGrid`): 2 cột trên điện thoại, 3 cột từ 768px, 4 cột từ 1024px (5 cột cho hàng "Món bán chạy"), gap 12/16px; mỗi ô là một gói kéo giãn đủ chiều cao hàng. Chi tiết sản phẩm: một mặt gói hai cột từ 768px (ảnh trái, mua phải, gap 40px), bên dưới mô tả 2/3 + hộp thông số 1/3. Giỏ hàng và thanh toán: một cột, từ 1024px tách `1fr + 340–360px` với thẻ tóm tắt dính (`sticky top-24`). Chi tiết đơn: `1fr + 320px`. Tài khoản: nav ngang cuộn trên điện thoại, cột trái 220px dính từ 768px.

Admin: dải sọc 10px trên cùng, sidebar trái 240px nền giấy viền phải (ẩn dưới 1024px, thay bằng drawer 256px), header 56px nền kem mờ, nội dung đệm 16/24px. Tổng quan: `2fr 3fr` từ 1280px với hộp doanh thu bên trái và ba ô số bên phải; biểu đồ và bảng hai cột từ 1280px.

Nhịp khoảng cách (Tailwind 4px): 4px trong nhóm nhỏ, 8px giữa nút/pill/sticker, 12px đệm ô bảng và gap gói trên điện thoại, 16px đệm thẻ, đệm gói và gap trong thẻ, 24px giữa các thẻ, đệm thẻ desktop và gap section, 32px đệm dọc trang, 48px giữa các section trang chủ. Gói sản phẩm đệm 16px với 28px trên để chừa dải ép; mặt gói chi tiết đệm 24/40px với 40/48px trên. Breakpoint là mặc định Tailwind (640 / 768 / 1024 / 1280px).

### Named Rules
**The Sealed-Top Rule.** Mọi gói màu (danh mục, sản phẩm, mặt gói chi tiết, nhãn kệ) có một dải `stripes-sm crimp` cao 16px (20px trên mặt gói lớn) dán sát mép trên, răng cưa 12px; nội dung bắt đầu dưới dải đó. Hộp giấy trắng không có dải.

**The Disc Rule.** Ảnh sản phẩm luôn là đĩa tròn (`rounded-full`, `object-cover`) nền giấy với viền trắng mờ 4px (`ring-4 ring-white/60`) trên kệ và 8px trên mặt gói/hero; không bao giờ là hình vuông hay chữ nhật bo góc trừ ảnh nhỏ trong giỏ, dòng đơn và mã QR.

## Elevation & Depth

Hệ thống phẳng ở trạng thái nghỉ: kệ kem ở dưới, hộp giấy trắng viền 1px `rule` và gói màu đặt lên trên, viên trắng và tem trắng dán lên gói. Gói có một vệt ánh bạc duy nhất (`bag::before`: gradient 120° trắng 16% → 0 ở 45%) mô phỏng giấy bạc; không gradient nào khác. Header và pill danh mục tách khỏi trang bằng nền kem mờ + `backdrop-blur` và viền dưới `rule`, không bóng. Bóng chỉ xuất hiện khi một thứ rời khỏi kệ: gói được nhấc (hover/focus) hoặc lớp nổi (dialog, drawer).

### Shadow Vocabulary
- **Gói nhấc khỏi kệ** (`--shadow-bag: 0 18px 30px -14px oklch(0.3 0.06 40 / 0.45)`): utility `lift` khi hover, focus-visible hoặc focus-within trên gói danh mục, gói sản phẩm và `StatTile` có liên kết; đi kèm `translateY(-3px) rotate(-0.6deg)` trong 240ms `--ease-out`.
- **Hộp nổi** (`--shadow-float: 0 24px 48px -16px oklch(0.3 0.06 40 / 0.4)`): `DialogContent` và drawer admin, kèm màn `chili-shadow` 40% + `backdrop-blur(2px)`.

### Named Rules
**The Lift-Only Shadow Rule.** Bóng là bằng chứng một gói đã rời kệ. Thẻ, nút, ô nhập, header, sticker, tem không có bóng khi nghỉ; chỉ `lift` (hover) và lớp nổi (dialog, drawer) được có bóng.

## Shapes

Hai họ hình: **gói** và **viên**. Góc bo lấy từ `--radius: 0.75rem`: 6px (`sm`) cho ảnh nhỏ trong giỏ và tag "-x%", 8px (`md`) cho sticker và ảnh dòng đơn, 10px (`lg`) cho ô nhập, ô phương thức thanh toán, Alert ghi chú và skeleton, 12px (`xl`) cho Alert, hộp chuyển khoản, dòng giỏ hàng và toast, 16px (`2xl`) cho thẻ, bảng, dialog, ô số, `EmptyState`, 28px (`3xl`, utility `bag`) cho mọi gói màu. Tròn hoàn toàn cho mọi điều khiển (nút, pill, mục nav, phân loại, tem trạng thái, viên giỏ hàng, ô tìm kiếm, đĩa số bước), đĩa ảnh và tem giá.

Hình cắt: dải sọc chéo 45° (`repeating-linear-gradient`, chu kỳ 56px ở băng thương hiệu, 20px ở dải ép) với mép dưới răng cưa bằng `mask: conic-gradient` 12px (`crimp`); ngôi sao 24 cánh bằng `clip-path: polygon` (`burst`) cho ưu đãi (144–176px) và con dấu "Bán chạy"/"Hết hàng" (44–56px). Tem giá là đĩa `aspect-ratio: 1` nền giấy, viền 3px `currentColor` lấy màu `rim` của gói (ớt/xoài đậm/lá dứa/cô-ban), chữ Baloo 2 800 cỡ nhỏ xếp hai dòng.

Viền: 1px `rule` cho hộp giấy; `foreground/15–25%` cho pill và nút outline trên kem (lên 60–100% khi hover); viền trắng 50% 2px cho pill phân loại và nút outline trên gói màu (trắng 100% khi hover, chọn thì tô trắng); 3px ớt cho gói Đồ uống; gạch đứt `foreground/25` cho `EmptyState`.

### Named Rules
**The Pill Rule.** Mọi điều khiển có trạng thái chọn (pill danh mục, mục nav, trang trong phân trang, phân loại, nút) là viên tròn hoàn toàn. Đang chọn thì tô đặc màu (ớt trên kem; màu của gói cho pill danh mục; trắng với chữ màu gói trên mặt gói); không chọn thì viền mảnh trên nền giấy hoặc viền trắng mờ trên gói. Không dùng gạch chân hay đổi màu chữ đơn thuần để chỉ trạng thái chọn.

## Components

### Buttons
Nhãn in trên gói: viên tròn hoàn toàn, chữ 14px đậm 700, cao 40px, ấn xuống 1px khi `active`, viền 1px trong suốt (giữ cùng cỡ với outline).
- **Shape:** tròn hoàn toàn (`rounded-full`); size `sm` 32px chữ 12px đệm 14px; `lg` 48px chữ 16px đệm 28px; `icon` 40×40, `icon-sm` 32×32. `link` bỏ bo và đệm.
- **Primary (`default`):** nền ớt, chữ trắng; hover ớt đậm. Dùng trên kem/giấy: Đặt hàng, Tiến hành thanh toán, Tiếp tục mua sắm, trang hiện tại phân trang, lưu trong admin.
- **Secondary:** nền xoài, chữ mực; hover xoài đậm. Là hành động chính **trên gói màu**: "Xem thực đơn", "Mua ngay", viên giỏ hàng (trên gói xoài thì dùng `default`).
- **Outline:** nền giấy, viền mực 25%, chữ mực; hover viền mực 100% và nền kem đậm. Phân trang, Đăng xuất admin, "Thử lại". Trên gói màu bị ghi đè thành nền trong suốt viền trắng 60% (`ProductPurchase` "Thêm vào giỏ").
- **Ghost:** không viền, không nền; hover kem đậm + chữ ớt đậm. Icon trong header (ghi đè chữ trắng, hover ớt đậm), xoá dòng giỏ, sao chép, "Xem cửa hàng".
- **Destructive:** nền ớt đậm, chữ trắng; hover `brightness(0.9)`. Luôn đi kèm dialog xác nhận.
- **Link:** chữ ớt đậm 600, không đệm, gạch chân khi hover, `underline-offset` 4px.
- **Focus:** ring 3px `ring` ở 40%; **Loading:** `Loader2` quay, `aria-busy`; **Disabled:** opacity 50%.
- **Motion:** chuyển màu/viền/transform 150ms.

### Chips
Hai nhãn in trong một `Badge`: sticker và tem.
- **Sticker (`shape="sticker"`):** bo 8px, không viền, đệm 2×8px, chữ 12px 700. Biến thể: `default` ớt đặc chữ trắng; `secondary` xoài chữ mực; `success`/`warning`/`destructive`/`info` nền nhuộm (`*-tint`) chữ màu đậm; `muted` kem đậm chữ mực nhạt; `outline` chỉ viền mực 30%. Dùng cho số tồn kho, nhãn phân loại.
- **Tem (`shape="stamp"`):** viên tròn hoàn toàn, nền màu đặc, chữ 11px 800 IN HOA tracking rộng, đệm 4×10px, không viền, không xoay. `success` lá dứa, `warning`/`secondary` xoài chữ mực, `info` cô-ban, `destructive` ớt đậm, `muted` kem đậm, `default` ớt. Chỉ cho trạng thái đã được đặt (đơn, thanh toán), không bấm được; `key={status}` để re-mount khi đổi.
- **Ánh xạ trạng thái** (`lib/labels.ts`): Chờ xác nhận → warning; Đã xác nhận / Đang chuẩn bị → info; Đang giao → secondary; Đã giao → success; Đã hủy → destructive. Thanh toán: Chưa / Chờ hoàn → warning; Đã thanh toán → success; Hoàn tiền → muted; Thất bại → destructive.

### Cards / Containers
Hộp giấy trắng trên kệ kem.
- **Corner Style:** 16px.
- **Background:** `paper`; `EmptyState` dùng `paper` 60% để thấy mờ kệ.
- **Shadow Strategy:** không khi nghỉ; `StatTile` có liên kết nhận `lift`.
- **Border:** 1px `rule`; `BankTransferBox` viền mực 40% để nổi hơn; `EmptyState` gạch đứt mực 25%; ô phương thức thanh toán đang chọn viền ớt + nền ớt nhuộm.
- **Internal Padding:** 16px, 24px từ 640px; gap trong thẻ 16px. `CardTitle` là `h2` Baloo 2 20px 800; bước thanh toán đặt `step-disc` (đĩa ớt 28px, số Baloo 2 15px trắng) trước tiêu đề.

### Bag (signature)
Gói snack: utility `bag` (bo 28px, `overflow: hidden`, `isolation`, vệt ánh bạc) + một màu `toneClasses[tone].bg/text` + dải `stripes-sm crimp` 16px dán mép trên + `lift`.
- **Gói danh mục** (trang chủ): cao tối thiểu 84px, đệm 20×16px, tên Baloo 2 20/24px + số sản phẩm 12px 600 ở 85–95% + mũi tên 20px. Danh mục thứ 5 là gói giấy viền ớt 3px chữ ớt đậm.
- **Gói sản phẩm** (`ProductCard`): toàn bộ là một `Link`, đệm 16px (28px trên), đĩa ảnh 96/112px viền trắng 60% ở góc trên trái, tem giá 68/80px góc trên phải, `h3` 16/18px 700, mô tả 12px ẩn trên điện thoại, pill "Chọn mua" trắng 32px chữ 12px 800 (hover xoài; trên gói xoài hover ớt chữ trắng), con dấu "Bán chạy" (xoài) hoặc "Hết hàng" (giấy chữ mực nhạt) ở góc dưới phải, hoặc "Đã bán n" 12px.
- **Mặt gói** (chi tiết sản phẩm, nhãn kệ danh mục, hero): đệm 24/40px với 40–48px trên, dải ép 20px, `h1` 36/48px `overprint-sm` (hero 48–72px `overprint`), chữ phụ ở 85–95%.

### Price stamp (signature)
`PriceStamp`: utility `stamp`: đĩa `aspect-ratio: 1` nền giấy, viền 3px `currentColor` (lớp `rim` của gói), chữ Baloo 2 800 line-height 1,05, đệm 4px; giá 11–18px tabular, dòng dưới "-x%" 11px 700 ớt đậm (ẩn trên điện thoại; `detail` thêm giá gốc gạch ngang). Trên kệ 68/80px, trên mặt gói 112px. `PriceTag` (giỏ, đơn, admin): giá Baloo 2 800 ớt đậm 20px (36px ở `lg`), giá gốc 14px mực nhạt gạch ngang, tag "-x%" nền ớt chữ trắng 12px bo 6px.

### Burst and seal (signature)
`burst`: ngôi sao 24 cánh `clip-path`, nền xoài chữ mực Baloo 2 16/18px 800, 144/176px, góc trên phải mặt gói hero; nội dung là ngưỡng freeship từ cài đặt công khai ("FREESHIP / TỪ 300K") hoặc "COD / HOẶC QR". Một ngôi sao lớn trên toàn kệ. `Seal` (`seal burst`): 44/56px, chữ 8–9,6px IN HOA, xoài chữ mực (trên gói xoài: ớt chữ trắng), giấy chữ mực nhạt cho "Hết hàng".

### Inputs / Fields
Ô in trên mặt sau gói: dải trắng viền ấm, chuyển sang ớt khi focus.
- **Style:** cao 40px, bo 10px, viền `input` ở 70%, nền giấy, chữ 14px, đệm 8×12px, placeholder mực nhạt. `NativeSelect` cùng lớp với đệm phải 32px. `Textarea` cao tối thiểu 80px. `Checkbox` 16px `accent-color` ớt; radio `accent-chili`.
- **Hover:** viền `input` 100%. **Focus:** viền ớt + ring 3px `ring` ở 25%.
- **Error:** `aria-invalid` → viền ớt đậm, ring ớt đậm 20%, thông báo 14px ớt đậm `role="alert"`. **Disabled:** nền kem đậm, opacity 70%, con trỏ cấm.
- **Search (header):** cao 44px, tròn hoàn toàn, nền giấy, viền 2px trong suốt → xoài khi focus, icon `Search` 16px mực nhạt bên trái, đệm trái 40px.
- **Label:** 14px, 500; dấu `*` ớt đậm cho trường bắt buộc; gợi ý 12px mực nhạt.
- **Phân loại trên mặt gói:** pill radio viền 2px trắng 50%, chữ 14px 700; chọn thì nền giấy viền giấy chữ màu gói; hết hàng opacity 60%; focus ring trắng 50%.

### Navigation
- **Header storefront:** băng `stripes` 72px, tên tiệm Baloo 2 30/36px `overprint` trắng, ô tìm kiếm trắng tối đa 448px, nút ghost trắng (hover ớt đậm), viên giỏ hàng `secondary` với icon `ShoppingBag` và số đếm tabular, `bump` khi số tăng.
- **Pill danh mục:** cao 36px, chữ 14px 700, tròn hoàn toàn, viền 2px; không chọn: nền giấy viền mực 15% (hover 60%); đang chọn: nền màu bao bì theo vị trí (`toneAt(index + 3)`, "Tất cả" là lá dứa) viền trong suốt. `aria-current="page"`.
- **Nav tài khoản và sidebar admin:** mục tròn hoàn toàn, đệm 8×16px, chữ 14px 600 kèm icon 16px; hover kem đậm + chữ ớt đậm; đang chọn ớt đặc chữ trắng. Sidebar admin 240px nền giấy viền phải, tên "SnackShop" ớt Baloo 2 24px + nhãn "Quầy" 10px IN HOA trên kem đậm.
- **Phân trang:** viên `icon-sm` 32px; trang hiện tại `default` ớt, các trang khác `outline`; mũi tên `ChevronLeft/Right`.
- **Breadcrumb:** 14px mực nhạt, phân cách "/", hover ớt đậm gạch chân, trang hiện tại chữ mực.
- **Footer:** mặt sau gói: dải sọc 12px, mảng ớt, tên tiệm Baloo 2 30px `overprint-sm`, tiêu đề cột 20px, chữ trắng 85%, pill freeship xoài, dòng bản quyền 12px trắng 75% trên viền trắng 20%.

### Table (quầy)
Hộp giấy bo 16px chứa `<table>` 14px. Đầu bảng nền kem đậm 70%, cao 40px, chữ 11px IN HOA 800 tracking rộng mực nhạt; hàng ngăn bằng `rule`, hover kem 80%; ô đệm 10×12px. Cột STT (`TableRowNumber`) rộng 40px, số 12px 700 mực nhạt. SKU dùng `font-mono` 12px.

### Dialog
Hộp nhấc khỏi kệ: rộng tối đa 512px, bo 16px, không viền, đệm 24px, bóng `shadow-float`, màn `chili-shadow` 40% + blur 2px; vào/ra bằng fade + zoom 98% (tw-animate-css). Tiêu đề Baloo 2 20px 800; nút đóng tròn góc trên phải (hover kem đậm + ớt đậm); footer xếp ngược trên điện thoại.

### Feedback
- **Alert:** mảng nhuộm bo 12px, đệm 12px, không viền, không dải dọc: info cô-ban nhuộm/chữ cô-ban đậm, success lá dứa nhuộm, error ớt nhuộm; icon lucide 16px; tiêu đề 700, nội dung mực 85%.
- **EmptyState:** viền gạch đứt mực 25%, nền giấy 60%, bo 16px, đệm 40px, icon `PackageOpen` 40px ớt, tiêu đề Baloo 2 20px.
- **ErrorState:** mảng ớt nhuộm bo 16px chữ ớt đậm, icon `AlertCircle` 32px, nút outline "Thử lại".
- **Skeleton:** kem đậm, `animate-pulse`, bo 10px. **Spinner:** `Loader2` ớt quay + chữ 14px mực nhạt.
- **Toast (sonner):** `richColors`, bo 12px, viền `rule`, font Be Vietnam Pro, trên giữa, có nút đóng.

### Stat tile (signature)
`StatLedger` + `StatTile`: lưới ô 2/4 cột (3 cột ở tổng quan), mỗi ô hộp giấy bo 16px đệm 16px: nhãn 14px 600, con số Baloo 2 30px 800 ớt đậm, gợi ý 12px. `tone="warning"` (có việc cần xử lý) đổi cả ô sang mảng xoài với con số chữ mực; ô có liên kết nhận `lift`. Hộp doanh thu: con số 48px ớt đậm trên danh sách `dl` 14px.

### Order timeline
`OrderTimeline`: `<ol>` các bước, mỗi bước lưới `28px 1fr`: đĩa `step-disc` (bước mới nhất ớt chữ trắng, các bước trước kem đậm chữ mực nhạt), tên trạng thái 14px (mới nhất 700 ớt đậm, trước đó 500 mực 80%), thời gian 12px mực nhạt, ngăn bằng `rule`. Trạng thái hiện tại trên đầu trang là tem; lịch sử không bao giờ hiển thị như nút bấm được.

### Chart
`SimpleBarChart` (recharts): một chuỗi, một màu `chart-1` (ớt), không legend; cột tối đa 40px bo đỉnh 4px, lưới ngang gạch đứt `rule`, nhãn trục 12px mực nhạt, tooltip nền giấy viền `rule` bo 8px font Be Vietnam Pro. Luôn đi kèm bảng dạng văn bản ("Xem dạng bảng").

## Do's and Don'ts

### Do:
- **Do** in mỗi gói (danh mục, sản phẩm, mặt gói chi tiết) bằng đúng một màu bao bì phủ toàn mặt, chọn qua `toneAt`/`toneFor`, với dải `stripes-sm crimp` dán mép trên và `lift` khi hover.
- **Do** đặt hành động chính là ớt chữ trắng trên kem/giấy và xoài chữ mực trên gói màu (ớt trên gói xoài); mọi điều khiển khác trên gói là viên trắng hoặc viền trắng mờ.
- **Do** in giá bằng tem tròn trắng viền 3px màu gói (`PriceStamp`) trên gói, và Baloo 2 800 ớt đậm (`PriceTag`) trên giấy; giảm giá ghi "-x%" ớt đậm hoặc tag ớt chữ trắng.
- **Do** hiển thị trạng thái đơn và thanh toán bằng `Badge shape="stamp"` (viên màu đặc, IN HOA) với ánh xạ trong `lib/labels.ts`; nhãn số liệu (tồn kho) bằng sticker nhuộm.
- **Do** dùng Baloo 2 800 cho mọi tiêu đề, giá và con số, và thêm `overprint`/`overprint-sm` chỉ khi tiêu đề nằm trên gói màu.
- **Do** cắt ảnh sản phẩm thành đĩa tròn viền trắng mờ trên gói; để ảnh nhỏ bo 6–8px trong giỏ và dòng đơn.
- **Do** giữ ring focus 3px `ring` 40% (trắng 50% trên gói màu), `aria-current="page"` trên mục đang chọn, icon lucide 16px `aria-hidden` kèm chữ.
- **Do** tôn trọng `prefers-reduced-motion`: `lift` và `bump` đã tắt; mọi motion mới cũng phải tắt theo.

### Don't:
- **Don't** thêm bóng cho thẻ, nút, ô nhập, header, sticker hay tem khi nghỉ; chỉ `lift` và lớp nổi (dialog, drawer) có bóng.
- **Don't** dùng Baloo 2 cho thân bài, nút, ô nhập, ô bảng hay nhãn form; không dùng Be Vietnam Pro cho `h1`–`h2`; không đưa mặt chữ viết tay nào trở lại.
- **Don't** đặt nút ớt lên gói ớt, hay dùng màu bao bì làm viền, gạch chân, icon lẻ trên nền kem; màu là mảng, không phải điểm nhấn.
- **Don't** thêm ngôi sao thứ hai trên cùng một kệ; `burst` lớn chỉ dành cho ưu đãi freeship/COD ở hero, `Seal` nhỏ chỉ cho "Bán chạy"/"Hết hàng".
- **Don't** dùng gradient nào ngoài dải sọc 45° và vệt ánh bạc của `bag`; không nền tối cho section.
- **Don't** thêm chế độ tối hay biến thể `dark:` cho token; hệ thống là kệ sáng.
- **Don't** quay lại thế giới vở: không nền kẻ ô, lề đỏ, con dấu nghiêng, leader chấm chấm hay danh sách đánh số thay cho lưới gói.
- **Don't** dùng emoji trong giao diện; dùng icon lucide hoặc chữ.
- **Don't** vẽ UI cho mã giảm giá, đánh giá sản phẩm hay cổng VNPay/MoMo như thể đã có (chưa có trong sản phẩm).
