const DEFAULT_REDIRECT_PATH = '/app/projects';

export function getSafeRedirectPath(redirect: string | null) {
  if (!redirect || !redirect.startsWith('/')) {
    return DEFAULT_REDIRECT_PATH;
  }

  if (redirect.startsWith('//') || /^[a-zA-Z][a-zA-Z\d+\-.]*:/.test(redirect)) {
    return DEFAULT_REDIRECT_PATH;
  }

  return redirect;
}