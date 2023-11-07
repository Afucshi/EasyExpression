## EasyExpression

依赖
- .netstandard 2.0

### 支持的功能
- 逻辑运算
- 算术运算
- 函数嵌入
- 优先级解构

### 函数扩展

在EasyExpression.FormulaAction下可以实现更多函数

### 逻辑运算
**0d** 表示 **false**
**1d** 表示 **true**

### 示例
- 逻辑表达式

```var expStr = "3 * (1 + 2) < = 5 || !(8 / (4 - 2) > [SUM](1,2,3))";
 var exp = new Expression(expStr);
 exp.LoadArgument();
 var value = exp.Excute();
 ```
- 算术表达式

 ```
 var expStr = "3 * (1 + 2) + 5 - (30 / (4 - 2) % [SUM](1,2,3))";
 var exp = new Expression(expStr);
 exp.LoadArgument();
var value = exp.Excute();
```