---
version: 1
slug: "frontend-src-app-storefront-page-tsx"
primary_target: "frontend/src/app/(storefront)/page.tsx"
related_targets: ["frontend/src/app/(storefront)/layout.tsx","frontend/src/components/admin/admin-shell.tsx","frontend/src/app/globals.css"]
---

# Surface brief: SnackShop storefront home (and the shared visual world for storefront + admin)

Scope: full redesign of the Next.js frontend (storefront routes under `(storefront)`, admin routes under `(admin)`). Replaces the "tập ô ly" world the user rejected on 2026-11-05 (grid paper, red margin, stamps, numbered lists: anti-reference). Light-tone pin released 2026-11-05. Mode: Persuade on the home page; Operate on listing, product, cart, checkout, account and every admin route; Read on order detail / success pages.
Audience: young Vietnamese snack buyers on phones; shop owner and staff on laptops.
Job: buy a few familiar snacks in under 2 minutes; process an order in under 5 clicks.
Content: real seed catalog (5 categories, 14 products, placeholder images from the API). Free-shipping threshold from public settings. No testimonials, no customer numbers, no promo codes; nothing invented.
Constraints: Tailwind v4 tokens in `globals.css`, Radix + lucide, Vietnamese copy, existing routes, forms and behaviour unchanged. Demo project: name "SnackShop" kept, no brand assets.
Decision channel: the impeccable decision page (serve-question). Round 1 (assigned "Bàn nhựa vỉa hè") was re-rolled by the user in the safer register; in round 2 the user locked the grounded card "Bao bì snack Việt" with its HTML sketch `.impeccable/mocks/decision/grounded-bao-bi.html` as the composition to match. Build path: code-led (no image generation).

## Direction contract

THESIS: SnackShop is the front of a Vietnamese offset-printed snack bag blown up to page size: saturated colour fields, a diagonal stripe band, one starburst that shouts the offer, and every category owning one packaging colour. It refuses the white marketplace template (banner over equal cards with an orange button) and the notebook metaphor it replaces.

OWN-WORLD: Cream ground is the shelf. Four packaging colours by category, used as whole fields never accents: chili red (khô gà & khô bò, also the brand/primary), mango yellow (bánh tráng, also the CTA on red), pandan green (hạt & trái cây sấy, also success/in stock), cobalt blue (kẹo & bánh ngọt, also info); đồ uống is cream with a red rim. Diagonal 45° red-yellow stripes are the brand band (header, admin top strip). The offer is a yellow starburst. Prices are round white stamps with a coloured rim. Controls on colour are white pills; the primary button is yellow with dark text on red, and chili red with white text on cream. Display face Baloo 2 800 with a hard 3px dark offset (overprint); Be Vietnam Pro for body and UI. Depth is the bag's soft drop shadow on lift; hairline borders only on cream. Admin: cream ground, the stripe strip at the top only, colour reserved for the active nav item, status chips, warning tiles and the primary action.

STORY: The visitor sees a shelf of bright bags; the biggest asks "Tối nay ăn vặt gì?", the burst names the free-shipping threshold, and a tap on a bag opens the snack where one more tap adds it to the basket; the cart pill counts it. Staff open the counter, scan colour-coded status chips, and move an order forward.

FIRST VIEWPORT (home, desktop 1440): stripe band header (72px) with SnackShop in Baloo with the offset shadow, a white rounded search, "Đăng nhập" and a mango cart pill with the count; under it a row of category pills. Below, left 60%: a red bag face (radius 28) holding the headline in Baloo 72px white, the mango starburst "FREESHIP TỪ {threshold}", one line of copy, the mango "Xem thực đơn" button, and the first featured product's image as a disc peeking from the bag's bottom-right corner. Right 40%: five category bags stacked, each in its packaging colour with a stripe strip at the top edge, name and product count. Beneath, a row of five featured product bags coloured by a stable hash of the product: image disc, name, round price stamp at the top-right, and a white "Chọn mua" pill that opens the product, where the variant is picked and "Thêm vào giỏ" adds it (a product summary carries no variant, so the add stays on the product page).

FORM: Bao bì snack Việt (Vietnamese offset-printed foil snack packaging); candidate 5 of 7 on the ordered grounded list, locked by the user from the safer hand of re-roll round 1. seed:09013e5638613c46

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance.

## Memorable moment
Hovering a bag lifts it off the shelf (translate, 0.6° rotation, soft offset shadow); adding to the cart makes the mango cart pill bump once. Everything else is a 150ms colour change.

## Unresolved
None pinned by the user. The user's open preferences: appetising, photo-forward, recognisably a snack shop, no handwriting faces.
