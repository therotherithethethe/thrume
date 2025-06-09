using Thrume.Api.Extensions;
using Thrume.Database;
using Thrume.Domain.Entity;

namespace Thrume.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

public class NotBannedRequirement : IAuthorizationRequirement;

public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
{
    private readonly UserManager<Account> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotBannedHandler(UserManager<Account> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotBannedRequirement requirement)
    {
        var userPrincipal = _httpContextAccessor.HttpContext?.User;
        if (userPrincipal == null)
        {
            context.Fail(); // Немає користувача
            return;
        }

        var user = await _userManager.GetUserAsync(userPrincipal);
        if (user == null)
        {
            context.Fail(); // Користувач не знайдений в базі
            return;
        }

        // Головна логіка: якщо користувач в ролі "Banned", він не проходить перевірку
        if (await _userManager.IsInRoleAsync(user, AppRoles.Banned))
        {
            context.Fail(new AuthorizationFailureReason(this, "User is banned."));
            return;
        }

        // Якщо не забанений, вимога виконана
        context.Succeed(requirement);
    }
}

public class ViewPostRequirement : IAuthorizationRequirement { }

public class ViewPostAuthorizationHandler : AuthorizationHandler<ViewPostRequirement, Post>
{
    private readonly UserManager<Account> _userManager;

    public ViewPostAuthorizationHandler(UserManager<Account> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ViewPostRequirement requirement, 
        Post resource) // `resource` - це об'єкт Post, до якого ми перевіряємо доступ
    {
        var user = await _userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return; // Немає користувача, нічого не робимо
        }

        // Правило 1: Адміністратори можуть переглядати будь-який пост
        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        // Правило 2: Власник може переглядати свій пост, НАВІТЬ ЯКЩО ВІН ЗАБАНЕНИЙ
        // Це правило спрацює до того, як ми перевіримо бан
        if (resource.Author.Id.Equals(user.Id))
        {
            context.Succeed(requirement);
            return;
        }
        
        // Правило 3: Звичайні користувачі (не власники) можуть переглядати пост,
        // тільки якщо вони не забанені
        if (await _userManager.IsInRoleAsync(user, AppRoles.User) && 
            !await _userManager.IsInRoleAsync(user, AppRoles.Banned))
        {
            context.Succeed(requirement);
            return;
        }

        // Якщо жодне правило не спрацювало, доступ заборонено
    }
}

public class PostAuthorizationFilter : IEndpointFilter
{
    private readonly IAuthorizationService _authService;
    private readonly AppDbContext _dbContext;

    public PostAuthorizationFilter(IAuthorizationService authService, AppDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Отримуємо postId з аргументів ендпоінту
        var postId = context.GetArgument<Guid>(0); // Припускаємо, що postId - перший аргумент
        if (postId == Guid.Empty)
        {
            return Results.BadRequest("Invalid post ID.");
        }

        var post = await _dbContext.PostDbSet.FindAsync(postId);
        if (post == null)
        {
            return Results.NotFound();
        }

        // Кладемо знайдений пост в контекст, щоб ендпоінт не шукав його знову
        context.HttpContext.Items["post"] = post;

        var authorizationResult = await _authService.AuthorizeAsync(context.HttpContext.User, post, "CanViewPost");

        if (!authorizationResult.Succeeded)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}