# 枚举与字典在API开发中的最佳实践 Demo

## 项目结构

```
EnumDictDemo/
├── Attributes/
│   └── DictTranslateAttribute.cs    # 字典翻译特性
├── Controllers/
│   ├── DemoController.cs            # 演示 API（用户/订单 CRUD）
│   └── EnumsController.cs           # 枚举查询 API
├── Data/
│   └── AppDbContext.cs              # EF Core 上下文 + 种子数据
├── Filters/
│   └── DictTranslateFilter.cs       # 响应拦截器（IAsyncResultFilter）
├── Infrastructure/
│   ├── DictTranslateOptions.cs      # 配置选项（IOptions）
│   ├── DictTranslationHelper.cs     # 批量翻译执行器
│   ├── ObjectVisitor.cs             # 递归对象遍历器
│   └── TranslationRequest.cs        # 翻译请求 DTO
├── Models/
│   ├── Dto/
│   │   ├── EnumOptionResponse.cs    # 枚举选项响应
│   │   ├── OrderResponse.cs         # 订单响应（含枚举+字典）
│   │   └── UserResponse.cs          # 用户响应（含字典翻译）
│   ├── Entities/
│   │   └── SysDict.cs               # 字典表实体
│   └── Enums/
│       ├── OrderStatus.cs           # 订单状态枚举
│       └── PaymentMethod.cs         # 支付方式枚举
├── Services/
│   ├── DictService.cs               # 字典服务（IMemoryCache 缓存）
│   ├── EnumInfoService.cs           # 枚举信息服务
│   ├── IDictService.cs              # 字典服务接口
│   └── IEnumInfoService.cs          # 枚举服务接口
├── Program.cs                       # 入口 + DI 注册
├── appsettings.json                 # 配置
└── EnumDictDemo.http                # HTTP 测试文件
```

## 启动方式

```bash
cd d:\Code\EnumAndDict\EnumDictDemo
dotnet run --urls http://localhost:5000
```

## 核心 API 端点

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/enums` | 获取所有枚举列表 |
| GET | `/api/enums/{enumName}` | 获取指定枚举（如 OrderStatus） |
| GET | `/api/demo/user` | 单个用户（字典翻译演示） |
| GET | `/api/demo/users` | 用户列表（批量翻译） |
| GET | `/api/demo/order` | 单个订单（枚举+字典+嵌套） |
| GET | `/api/demo/orders` | 订单列表（完整嵌套翻译） |
| POST | `/api/demo/validate-dict-value` | 校验字典值合法性 |
| POST | `/api/demo/refresh-cache` | 刷新字典缓存 |

## 核心设计要点

### 1. 枚举方案
- 使用 `[Display(Name = "中文")]` 标注中文标签
- `EnumInfoService` 启动时自动扫描所有枚举并缓存
- 使用 `FastEnum` 库（`FastEnumUtility` 命名空间）避免反射，提升性能
- API: `GET /api/enums/{enumName}` 返回 `{ value, label, name }`

### 2. 字典翻译方案
- 使用 `[DictTranslate(DictCode, TargetProperty)]` 特性标记
- `DictTranslateFilter` 作为 `IAsyncResultFilter` 拦截所有响应
- `ObjectVisitor` 递归遍历对象树，支持嵌套对象、集合
- `DictTranslationHelper` 批量查询翻译，避免 N+1 问题
- `DictService` 使用 `IMemoryCache` 缓存，`SemaphoreSlim` 防止缓存击穿

### 3. 已验证功能
- ✅ 单层 DTO 字典翻译
- ✅ `List<T>` 集合翻译
- ✅ 嵌套对象递归翻译（OrderResponse 包含 UserResponse）
- ✅ 枚举 Label 填充
- ✅ 循环引用检测（`HashSet<object>` + `ReferenceEqualityComparer`）
- ✅ 递归深度限制（可配置，默认 10 层）
- ✅ 批量翻译优化（一次查询所有字典值）
- ✅ 缓存失效/刷新 API
- ✅ 字典值校验 API