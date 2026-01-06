# 🎟️ PROMOTION + ORDER PRICING SYSTEM - HƯỚNG DẪN & QUY TẮC

Tài liệu này mô tả **logic áp dụng khuyến mãi (Promotion)** và **tính giá đơn hàng (Order)** theo code hiện tại (OrderService/PromotionService).

> Mục tiêu chính:
> - **Product/Category promotions được auto-apply** vào `OrderItem.UnitPrice` khi tạo/cập nhật items.
> - **Order promotions được chọn thủ công** (client truyền `promotionIds`) và chỉ chấp nhận **scope = Order**.
> - Khi có nhiều promotion cùng scope/quan hệ, hệ thống luôn chọn **best discount (max %)**.

---

##1) Entities & Scopes

###1.1 Promotion scopes
`Promotion.Scope` thuộc enum `PromotionScope`:
- `Product`: áp dụng cho sản phẩm cụ thể (qua bảng `ProductPromotions`)
- `Category`: áp dụng cho danh mục (qua bảng `CategoryPromotions`)
- `Order`: áp dụng trực tiếp lên tổng đơn (qua bảng `OrderPromotions`)

###1.2 Bảng quan hệ
- `ProductPromotions(ProductId, PromotionId)`
- `CategoryPromotions(CategoryId, PromotionId)`
- `OrderPromotions(OrderId, PromotionId)`

---

##2) Quy tắc áp dụng Promotion

###2.1 Điều kiện promotion hợp lệ
Một promotion được coi là hợp lệ để áp dụng tại thời điểm `at` khi:
- `StartDate <= at <= EndDate`
- `DiscountPercent` nằm trong `0..100`

###2.2 Nguyên tắc chọn "best"
Nếu có nhiều promotion có thể áp dụng cho cùng đối tượng:
- Chọn promotion có `DiscountPercent` **lớn nhất**.
- Nếu bằng nhau (tie), code sử dụng lựa chọn **ổn định/nhất quán** bằng `PromotionId` nhỏ hơn (đã implement cho GetOrderById);

---

##3) Dòng tiền & kiểu dữ liệu (Anti-overflow)
Giá tiền lưu trong DB hiện tại là `int` (VND). Khi tính toán:
- Các phép nhân/tổng được thực hiện bằng `long` để tránh tràn số.
- Những điểm nguy cơ overflow đã được xử lý trong `OrderService` (subtotal, line total, discount amount).

---

##4) Flow tạo đơn (CreateOrder)
API: `createOrder(input)`

###4.1 Input quan trọng
- `items`: danh sách sản phẩm + số lượng
- `promotionIds`: **chỉ dành cho Order-scope** (client chọn)

###4.2 Các bước tính giá
1) **Validate items**: quantity >0, product tồn tại.
2) **Auto pricing** cho từng product:
 - Lấy `SalePrice` base.
 - Tìm best discount percent từ:
 - product promotions (scope Product)
 - category promotions (scope Category)
 - `UnitPrice = ApplyDiscount(SalePrice, bestPct)`
3) Tính line total:
 - `LineTotal = UnitPrice * Quantity` (long)
4) Tính `Subtotal = sum(LineTotal)` (long)
5) Validate `promotionIds` do client gửi:
 - chỉ cho phép scope = `Order`.
 - phải active tại thời điểm tạo.
6) Tính order discount:
 - lấy `bestPct = max(DiscountPercent)` trong danh sách order promo ids.
 - `OrderDiscountAmount = Subtotal * bestPct /100`
7) Tính `TotalPrice` lưu DB:
 - `TotalPrice = Subtotal - OrderDiscountAmount`
8) Persist:
 - `Orders` + `OrderItems` + `OrderPromotions`.

> Ghi chú: Product/Category promotions **không được lưu** vào bảng quan hệ theo order, vì đã được “chốt” vào `OrderItem.UnitPrice`.

---

##5) Flow cập nhật đơn (UpdateOrder)
API: `updateOrder(orderId, input)`

###5.1 Rule quan trọng
- **Không cho update** đơn `Status == Paid`.

###5.2 Trường hợp update

#### A) Update chỉ promotions (Order-scope)
- Remove toàn bộ `OrderPromotions` cũ
- Add `OrderPromotions` mới (đã validate scope/order/active)
- **Không** recalc unit prices
- Recalc `TotalPrice = Subtotal(old items) - orderDiscount(best pct)`

#### B) Update items
- Xóa items cũ, tạo items mới
- Recompute `UnitPrice` theo auto product/category promotions tại thời điểm update
- Recalc subtotal
- Recalc order discount theo current `OrderPromotions`

---

##6) Get Order by id: PromotionIds trả về là gì?
API: `orderById(orderId)`

`OrderDetailDto.PromotionIds` là danh sách IDs promotion **đại diện cho các ưu đãi đã áp dụng**.
Logic hiện tại:
- **Order scope**: chỉ trả về **1 promotion best** trong các `OrderPromotions` gắn với order.
- **Product scope**: trả về best promo cho **mỗi product** trong order (nếu có).
- **Category scope**: trả về best promo cho **mỗi category** xuất hiện trong order (nếu có).

Mục tiêu:
- Không trả về nhiều promotion cùng1 category nếu chỉ1 cái thực sự áp dụng.
- Dữ liệu trả về phù hợp với “best discount per scope”.

---

##7) FAQ / Lỗi hay gặp

###7.1 Vì sao CreateOrder chỉ nhận Order promotions mà không nhận Product/Category?
Vì Product/Category promotions được auto-apply để tạo `UnitPrice`. Nếu client gửi thêm sẽ gây mâu thuẫn/2 lần giảm.

###7.2 Vì sao GetOrderById có thể trả nhiều promotionIds?
Nếu order có:
- nhiều products khác nhau có best product promo khác nhau
- nhiều categories khác nhau, mỗi category có best promo
=> `PromotionIds` sẽ có nhiều id (nhưng mỗi scope/category chỉ lấy best).

###7.3 Tại sao TotalPrice không khớp với promo hiện tại (đang active)?
Order pricing được “chốt” tại thời điểm tạo/cập nhật:
- `OrderItem.UnitPrice` đã là giá sau product/category promo ở thời điểm đó.
- Nếu promotion thay đổi sau đó, giá đơn **không tự động đổi**.

---

##8) Gợi ý endpoint & test nhanh

Trong `GRAPHQL_TEST_QUERIES.http`:
- Promotions: create/list/update/delete
- Orders: create/list/get/update/delete

Test các rule:
1) Create order với nhiều product/category promotions => unit price đúng.
2) Với2 order promotions cùng lúc => total chỉ áp dụng best.
3) Update order chỉ promotions => unit price giữ nguyên.
4) Update items => unit price recalculated.

---

##9) Notes kỹ thuật

- `PriceCalc.ApplyDiscount(int salePrice, int discountPct)` dùng `decimal` để tính chính xác và làm tròn.
- Các phép cộng/nhân tiền trong order dùng `long` trung gian để tránh overflow.

