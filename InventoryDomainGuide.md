# Inventory Management System Domain Guide

ده شرح مبسط للكلاسات الأساسية في مشروع المخازن، وليه كل كلاس موجود، وإزاي المنتجات والمخازن والمخزون والفواتير مرتبطين ببعض.

## الفكرة العامة

المشروع مش مجرد Products CRUD. الفكرة الأقوى إن النظام يعرف:

- المنتج إيه.
- المنتج بيتقاس بإيه.
- المنتج موجود في أنهي مخزن.
- الكمية كام في كل مخزن.
- الكمية زادت أو قلت بسبب إيه.
- الفواتير اللي دخلت أو خرجت منها البضاعة.

أهم قاعدة:

```text
الكمية لا تتغير عشوائيًا.
أي تغيير في الكمية لازم يبقى وراه حركة مخزون StockMovement.
```

## Common

### Entity

المسار:

```text
InventoryManagementSystem.Domain/Common/Entity.cs
```

ده الأب الأساسي لمعظم كيانات الدومين.

أي كلاس يرث من `Entity` بياخد:

- `Id` نوعه `Guid`.
- قائمة `DomainEvents`.

استخدام `Guid` مفيد لأن كل كيان يبقى له رقم فريد قوي، ومناسب لو النظام كبر أو اتقسم لخدمات.

### DomainEvent

المسار:

```text
InventoryManagementSystem.Domain/Common/DomainEvent.cs
```

ده أساس لأي حدث يحصل داخل الدومين.

مثال مستقبلي:

```text
ProductCreated
StockQuantityChanged
InvoicePosted
```

حاليًا هو جاهز للاستخدام لاحقًا، وفيه وقت حدوث الحدث:

```csharp
OccurredOnUtc
```

### AuditableEntity

المسار:

```text
InventoryManagementSystem.Domain/Common/AuditableEntity.cs
```

ده يرث من `Entity` ويضيف بيانات التتبع:

- `CreatedAtUtc`
- `CreatedBy`
- `LastModifiedUtc`
- `LastModifiedBy`

يعني نعرف السجل اتعمل إمتى واتعدل إمتى.

### Result / Error / Updated

المسار:

```text
InventoryManagementSystem.Domain/Common/Results
```

دي طريقة المشروع في إرجاع نتيجة من الدومين.

بدل ما نرمي Exception في كل validation، بنرجع:

```csharp
Result<Product>
Result<Updated>
Error
```

مثال:

```csharp
if (price < 0)
    return ProductErrors.PriceInvalid;
```

ولو العملية نجحت:

```csharp
return Result.Updated;
```

## Product

### Product

المسار:

```text
InventoryManagementSystem.Domain/Product/Product.cs
```

ده يمثل المنتج نفسه.

فيه:

- `Name`: اسم المنتج.
- `Description`: وصف اختياري.
- `UnitId`: وحدة القياس.
- `Price`: السعر.
- `Quantity`: الكمية الحالية الموجودة في المنتج.

ملاحظة مهمة:

في التصميم الاحترافي، الكمية الأفضل تكون في `StockItem` لأنها تختلف من مخزن لمخزن. وجود `Quantity` داخل `Product` حاليًا بسيط ومقبول كبداية، لكن مع المخازن المتعددة الأفضل الاعتماد على `StockItem.QuantityOnHand`.

المنتج مربوط بالوحدة عن طريق:

```csharp
public Guid UnitId { get; private set; }
```

يعني المنتج لا يحمل كائن `Unit` نفسه، لكنه يحمل رقم الوحدة.

مثال:

```text
Product: Sugar
Unit: Kg
```

### ProductErrors

المسار:

```text
InventoryManagementSystem.Domain/Product/ProductErrors.cs
```

ده مكان أخطاء المنتج والوحدة والتصنيف.

مثال:

- `NameRequired`
- `PriceInvalid`
- `QuantityInvalid`
- `UnitRequired`
- `CategoryRequired`
- `DuplicateName`

وجود الأخطاء في ملف واحد يجعل الكود أنظف وأسهل في التعديل والترجمة لاحقًا.

### Category

المسار:

```text
InventoryManagementSystem.Domain/Product/Category.cs
```

ده تصنيف المنتج.

أمثلة:

- Food
- Electronics
- Clothes
- Medicine

التصنيف مهم للبحث والتقارير.

مثال تقرير:

```text
إجمالي قيمة مخزون الإلكترونيات
عدد منتجات الأطعمة القريبة من النفاد
```

### Unit

المسار:

```text
InventoryManagementSystem.Domain/Product/Unit.cs
```

ده وحدة القياس.

أمثلة:

- Piece / Pcs
- Kilogram / Kg
- Box
- Liter / L

لازم يبقى عندنا Unit لأن المنتجات مش كلها بتتقاس بنفس الطريقة.

مثال:

```text
سكر: Kg
موبايل: Piece
مياه: Box
زيت: Liter
```

## Warehouse

### Warehouse

المسار:

```text
InventoryManagementSystem.Domain/Warehouse/Warehouse.cs
```

ده يمثل المخزن.

فيه:

- `Name`: اسم المخزن.
- `Address`: عنوان اختياري.

مثال:

```text
Cairo Warehouse
Alexandria Warehouse
Main Store
```

وجود المخزن مهم لأن نفس المنتج ممكن يكون موجود في أكتر من مكان بكميات مختلفة.

مثال:

```text
Sugar:
- Cairo Warehouse: 100 Kg
- Alexandria Warehouse: 40 Kg
```

### WarehouseErrors

المسار:

```text
InventoryManagementSystem.Domain/Warehouse/WarehouseErrors.cs
```

ده مكان أخطاء المخازن.

مثال:

- `WarehouseNotFound`
- `NameRequired`

## Stock

### StockItem

المسار:

```text
InventoryManagementSystem.Domain/Stock/StockItem.cs
```

ده أهم كلاس في المخزون.

هو بيربط:

- منتج معين `ProductId`.
- بمخزن معين `WarehouseId`.
- وبكمية موجودة `QuantityOnHand`.

مثال:

```text
ProductId: Sugar
WarehouseId: Cairo Warehouse
QuantityOnHand: 100
```

يعني `StockItem` بيجاوب على سؤال:

```text
عندي كام من المنتج ده في المخزن ده؟
```

فيه عمليات:

```csharp
AddQuantity
RemoveQuantity
AdjustQuantity
```

### StockMovement

المسار:

```text
InventoryManagementSystem.Domain/Stock/StockMovement.cs
```

ده سجل حركة المخزون.

كل مرة الكمية تزيد أو تقل، نسجل حركة.

فيه:

- `ProductId`
- `WarehouseId`
- `Type`
- `Quantity`
- `ReferenceNumber`
- `Notes`

مثال:

```text
Purchase 50 Kg Sugar
Sale 10 Kg Sugar
Adjustment 5 Kg
```

الفائدة:

- تقدر تعرف الكمية اتغيرت ليه.
- تقدر تعمل تقرير حركة منتج.
- تقدر تراجع الأخطاء.
- تقدر تعرف مصدر كل زيادة أو نقص.

### StockMovementType

المسار:

```text
InventoryManagementSystem.Domain/Stock/StockMovementType.cs
```

ده enum لأنواع حركة المخزون:

- `Purchase`: شراء، تزود المخزون.
- `Sale`: بيع، تقلل المخزون.
- `Return`: مرتجع.
- `TransferIn`: تحويل داخل للمخزن.
- `TransferOut`: تحويل خارج من المخزن.
- `Adjustment`: تعديل جرد.

### StockErrors

المسار:

```text
InventoryManagementSystem.Domain/Stock/StockErrors.cs
```

ده مكان أخطاء المخزون.

مثال:

- المنتج مطلوب.
- المخزن مطلوب.
- الكمية غير صحيحة.
- الكمية غير كافية.

مثال مهم:

لو بتحاول تبيع 20 قطعة، لكن الموجود 5 فقط:

```csharp
return StockErrors.InsufficientQuantity;
```

## Invoices

الفواتير هي المستندات اللي بتثبت دخول أو خروج البضاعة.

عندنا نوعين أساسيين:

- فاتورة شراء `PurchaseInvoice`.
- فاتورة بيع `SalesInvoice`.

### InvoiceStatus

المسار:

```text
InventoryManagementSystem.Domain/Invoices/InvoiceStatus.cs
```

حالة الفاتورة:

- `Draft`: مسودة، ينفع تضيف وتعدل.
- `Posted`: اتعتمدت، المفروض تأثر على المخزون.
- `Cancelled`: اتلغت.

أهم فكرة:

```text
الفاتورة وهي Draft مجرد مستند مفتوح.
الفاتورة لما تبقى Posted تبدأ تؤثر على المخزون.
```

### InvoiceErrors

المسار:

```text
InventoryManagementSystem.Domain/Invoices/InvoiceErrors.cs
```

ده مكان أخطاء الفواتير.

مثال:

- رقم الفاتورة مطلوب.
- المخزن مطلوب.
- المنتج مطلوب.
- الكمية غير صحيحة.
- السعر غير صحيح.
- الفاتورة فاضية.
- الفاتورة اتعملها Post قبل كده.
- ممنوع تعديل فاتورة Posted.

### PurchaseInvoice

المسار:

```text
InventoryManagementSystem.Domain/Invoices/PurchaseInvoice.cs
```

دي فاتورة شراء.

يعني لما تشتري بضاعة من مورد.

فيها:

- `InvoiceNumber`: رقم الفاتورة.
- `WarehouseId`: المخزن اللي البضاعة هتدخل فيه.
- `SupplierId`: المورد، اختياري حاليًا.
- `InvoiceDateUtc`: تاريخ الفاتورة.
- `Status`: حالة الفاتورة.
- `Items`: بنود الفاتورة.
- `Total`: إجمالي الفاتورة.

مثال:

```text
فاتورة شراء رقم PUR-001
المخزن: Cairo Warehouse
المورد: Supplier A

Items:
- Sugar, Quantity 100, UnitCost 30
- Rice, Quantity 50, UnitCost 25

Total = 4250
```

لما فاتورة الشراء تعمل `Post`، المفروض في مرحلة التطبيق Application/Infrastructure نزود المخزون في `WarehouseId`.

### ليه WarehouseId موجود في فاتورة المشتريات؟

لأن النظام لازم يعرف البضاعة اللي اشتريتها هتدخل أنهي مخزن.

مثال:

```text
اشتريت 100 كرتونة مياه
```

لو عندك مخازن:

```text
Cairo Warehouse
Giza Warehouse
Alexandria Warehouse
```

لازم تحدد:

```text
الـ 100 كرتونة يدخلوا مخزن القاهرة ولا الجيزة؟
```

لو الفاتورة فيها:

```csharp
WarehouseId = CairoWarehouseId
```

يبقى بعد `Post` المخزون يزيد في مخزن القاهرة فقط.

بدون `WarehouseId`، السيستم هيعرف إنك اشتريت بضاعة، لكنه مش هيعرف يحطها فين.

ملاحظة تصميم:

حاليًا `WarehouseId` موجود في رأس الفاتورة، وده معناه إن كل بنود الفاتورة تدخل نفس المخزن.

لو عايزين فاتورة واحدة توزع منتجاتها على أكتر من مخزن، ممكن لاحقًا ننقل `WarehouseId` إلى `PurchaseInvoiceItem`.

### PurchaseInvoiceItem

المسار:

```text
InventoryManagementSystem.Domain/Invoices/PurchaseInvoiceItem.cs
```

ده بند واحد داخل فاتورة الشراء.

فيه:

- `ProductId`: المنتج.
- `Quantity`: الكمية.
- `UnitCost`: تكلفة الوحدة.
- `Total`: الإجمالي.

الحساب:

```csharp
Total = Quantity * UnitCost;
```

### SalesInvoice

المسار:

```text
InventoryManagementSystem.Domain/Invoices/SalesInvoice.cs
```

دي فاتورة بيع.

يعني لما تبيع بضاعة لعميل.

فيها:

- `InvoiceNumber`: رقم الفاتورة.
- `WarehouseId`: المخزن اللي البضاعة هتطلع منه.
- `CustomerId`: العميل، اختياري حاليًا.
- `InvoiceDateUtc`: تاريخ الفاتورة.
- `Status`: حالة الفاتورة.
- `Items`: بنود الفاتورة.
- `Total`: إجمالي الفاتورة.

مثال:

```text
فاتورة بيع رقم SAL-001
المخزن: Cairo Warehouse
العميل: Customer A

Items:
- Sugar, Quantity 10, UnitPrice 40
- Rice, Quantity 5, UnitPrice 35

Total = 575
```

لما فاتورة البيع تعمل `Post`، المفروض تقلل المخزون من `WarehouseId`.

### SalesInvoiceItem

المسار:

```text
InventoryManagementSystem.Domain/Invoices/SalesInvoiceItem.cs
```

ده بند واحد داخل فاتورة البيع.

فيه:

- `ProductId`: المنتج.
- `Quantity`: الكمية.
- `UnitPrice`: سعر البيع.
- `Total`: الإجمالي.

الحساب:

```csharp
Total = Quantity * UnitPrice;
```

## العلاقة بين الفواتير والمخزون

الفواتير لا تغير المخزون مباشرة داخل الكلاس نفسه حاليًا.

لكن القاعدة اللي هنكمل عليها:

### عند اعتماد فاتورة شراء

```text
PurchaseInvoice.Post()
```

لكل item:

```text
StockItem.AddQuantity(item.Quantity, StockMovementType.Purchase, invoice.InvoiceNumber)
```

يعني:

```text
شراء = زيادة مخزون
```

### عند اعتماد فاتورة بيع

```text
SalesInvoice.Post()
```

لكل item:

```text
StockItem.RemoveQuantity(item.Quantity, StockMovementType.Sale, invoice.InvoiceNumber)
```

يعني:

```text
بيع = نقص مخزون
```

لو الكمية غير كافية:

```text
يرجع StockErrors.InsufficientQuantity
```

## مثال كامل

### 1. إنشاء وحدة

```text
Unit: Kilogram, Symbol: Kg
```

### 2. إنشاء منتج

```text
Product: Sugar
Unit: Kg
Price: 40
```

### 3. إنشاء مخزن

```text
Warehouse: Cairo Warehouse
```

### 4. إنشاء StockItem

```text
Product: Sugar
Warehouse: Cairo Warehouse
QuantityOnHand: 0
```

### 5. فاتورة شراء

```text
PurchaseInvoice PUR-001
Warehouse: Cairo Warehouse

Items:
- Sugar, Quantity 100, UnitCost 30
```

بعد `Post`:

```text
StockItem.QuantityOnHand = 100
StockMovement = Purchase 100
```

### 6. فاتورة بيع

```text
SalesInvoice SAL-001
Warehouse: Cairo Warehouse

Items:
- Sugar, Quantity 20, UnitPrice 40
```

بعد `Post`:

```text
StockItem.QuantityOnHand = 80
StockMovement = Sale 20
```

## ملخص سريع

```text
Product
يعرف المنتج نفسه.

Unit
يعرف المنتج بيتقاس بإيه.

Category
يصنف المنتج.

Warehouse
يعرف مكان التخزين.

StockItem
يعرف كمية منتج معين داخل مخزن معين.

StockMovement
يسجل سبب أي زيادة أو نقص.

PurchaseInvoice
مستند شراء يدخل بضاعة.

SalesInvoice
مستند بيع يخرج بضاعة.
```

## الخطوة التالية المقترحة

نضيف Application layer للفواتير:

```text
CreatePurchaseInvoice
PostPurchaseInvoice
CreateSalesInvoice
PostSalesInvoice
```

وفي `PostPurchaseInvoice` و `PostSalesInvoice` نربط الفاتورة فعليًا بالمخزون ونحفظ `StockMovement`.
