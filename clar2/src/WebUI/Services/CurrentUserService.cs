﻿using System.Security.Claims;
using clar2.Application.Common.Interfaces;

namespace clar2.WebUI.Services;

public class CurrentUserService : ICurrentUserService {
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CurrentUserService(IHttpContextAccessor httpContextAccessor) {
    _httpContextAccessor = httpContextAccessor;
  }

  public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}