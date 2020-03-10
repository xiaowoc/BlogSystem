### 项目名称及简介
1. **项目名称：简易博客**

1. **技术类型：Asp.Net MVC5、三层架构、EF6、Nlog、JQuery、Ajax、Bootstrap**

1. **示例：134.175.185.129**

1. **相关功能：注册、登陆、创建文章、编辑文章、个人文章列表、创建文章所属分类、个人文章分类列表、修改个人信息、修改密码、密码重置、文章点赞点踩、文章评论、用户关注、自定义错误页面、错误日志记录、搜索**

### 项目配置指南

**在`BlogSystem / BlogSystem.MVCSite / Web.config`中配置连接数据库地址和相关数据**

```xml
<connectionStrings>
    <add name="con" connectionString="（数据库链接地址）" providerName="System.Data.SqlClient" ></add>
    <!--连接数据库配置-->
  </connectionStrings>
```

```xml
<appSettings>
    <add key="Url" value="（当前的地址或者服务器地址）" />
    <!--邮箱附带的重置密码连接地址-->
    <add key="Secret" value="（jwt密钥，可填写任意字符串）" />
    <!--jwt密钥-->
    <add key="EmailName" value="（用于发邮件的邮箱账号，目前配置的必须为QQ邮箱）" />
    <!--用来发重置密码邮件的邮箱账号-->
    <add key="EmailPass" value="（QQ邮箱开启POP3/SMTP服务后提供的授权码）" />
    <!--对应的邮箱授权码（不是密码）-->
  </appSettings>
```

**在`BlogSystem/BlogSystem.MVCSite/NLog.config`中配置Nlog生成的位置**

```xml
    <target name="file" xsi:type="File"
           layout="..." fileName="${basedir}/Logs/${shortdate}.log"
		   <!--可重新设置日志生成位置（非必须），生成的位置必须有权限修改-->
           keepFileOpen="true"
           encoding="utf-8" />
  </targets>
```

**`BlogSystem/BlogSystem.MVCSite/Image/`文件夹必须有权限修改**

**在`BlogSystem / BlogSystem.Models /App.config`中配置EF迁移服务的数据库链接**

```xml
<connectionStrings>
    <add name="con" connectionString="（数据库链接地址）" providerName="System.Data.SqlClient" />
  </connectionStrings>
```

**-----2020-3-10更新-----**

项目为了兼容vue，在部分内容中增加配置

**在`BlogSystem / BlogSystem.MVCSite / Web.config`中配置重置密码对应的链接**

将原来的：
```xml
<appSettings>
    <add key="Url" value="（当前的地址或者服务器地址）" />
    <!--邮箱附带的重置密码连接地址-->
  </appSettings>
```
更改为：
```xml
<appSettings>
    <add key="MvcUrl" value="（mvc部署地址）" />
    <add key="ApiUrl" value="（vue部署的地址）" />
    <!--邮箱附带的重置密码连接地址-->
  </appSettings>
```

**在相应的控制器中配置允许跨域的地址**

以下为HomeController的例子：
```csharp
[ControllerAllowOrigin(AllowSites = new string[] { "（地址1）", "（地址2）"})]
    public class HomeController : Controller
    {
	}
```


**配置完成后可以通过EF的迁移服务生成相应的数据库！**
