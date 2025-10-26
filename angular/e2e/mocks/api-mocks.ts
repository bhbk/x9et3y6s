import { Page, Route } from '@playwright/test';
import {
  ISSUERS,
  AUDIENCES,
  ROLES,
  USERS,
  CLAIMS,
  LOGIN_PROVIDERS,
  USER_ACTIVITIES,
  REFRESHES,
  ENTITLEMENTS,
  AUDIENCE_ENTITLEMENTS,
  QUOTE,
  QUOTES,
  EMAIL_QUEUE,
  TEXT_QUEUE,
  JOBS,
  LLM_PROVIDERS,
  PROMPT_HISTORY,
  CHAT_FAVORITES,
  pagedResult,
} from './mock-data';

// API base URLs that the Angular apps use (from config.json)
const STS_API = 'http://localhost:55114/api';
const ADMIN_API = 'http://localhost:55106/api';
const USER_API = 'http://localhost:55109/api';

/** Helper to respond with JSON */
function json(route: Route, body: unknown, status = 200) {
  return route.fulfill({
    status,
    contentType: 'application/json',
    body: JSON.stringify(body),
  });
}

/** Helper to respond with empty 200 (JSON content-type so Angular's withFetch() resolves) */
function ok(route: Route) {
  return route.fulfill({ status: 200, contentType: 'application/json', body: 'null' });
}

/** Helper to respond with 400 (OAuth2 token error per RFC 6749 §5.2) */
function badRequest(route: Route) {
  return route.fulfill({
    status: 400,
    contentType: 'application/json',
    body: JSON.stringify({ error: 'invalid_grant', error_description: 'Authentication failed' }),
  });
}

// ── STS Mocks ────────────────────────────────────────────────────────

/** Mock the ROPG login endpoint — succeeds for known users, fails otherwise */
export async function mockStsLogin(page: Page) {
  await page.route(`${STS_API}/oauth2/v2/ropg`, async (route) => {
    const request = route.request();
    const body = request.postData() ?? '';
    const params = new URLSearchParams(body);
    const user = params.get('user');
    const password = params.get('password');

    // Accept admin@local and user@local with any password
    if (user === 'admin@local' || user === 'user@local') {
      if (password === 'bad') {
        return badRequest(route);
      }
      return json(route, {
        token_type: 'bearer',
        access_token: createMockJwt(user),
        expires_in: 3600,
        issuer: 'Bhbk',
        client: [],
        user,
      });
    }

    return badRequest(route);
  });
}

/** Mock the password reset request endpoint (public, STS) */
export async function mockStsPasswordReset(page: Page) {
  await page.route(`${STS_API}/oauth2/v1/password/reset`, (route) => ok(route));
  await page.route(`${STS_API}/oauth2/v1/password/reset/confirm`, (route) => ok(route));
}

/** Mock the refresh token endpoint */
export async function mockStsRefresh(page: Page) {
  await page.route(`${STS_API}/oauth2/v2/ropg-rt`, async (route) => {
    return json(route, {
      token_type: 'bearer',
      access_token: createMockJwt('admin@local'),
      expires_in: 3600,
      issuer: 'Bhbk',
      client: [],
      user: 'admin@local',
    });
  });
}

// ── Admin API Mocks ──────────────────────────────────────────────────

/** Mock all admin API routes — call this once per test that needs admin API */
export async function mockAdminApi(page: Page) {
  // Issuers
  await page.route(`${ADMIN_API}/issuers/v1/page`, (route) =>
    json(route, pagedResult(ISSUERS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/issuers/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, ISSUERS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, ISSUERS[0]); // PUT
  });
  await page.route(`${ADMIN_API}/issuers/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      // Create — return the posted data merged with generated fields
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        ...body,
        created: new Date().toISOString(),
      });
    }
    if (method === 'PUT') {
      const body = route.request().postDataJSON();
      return json(route, body);
    }
    return route.fallback();
  });

  // Audiences
  await page.route(`${ADMIN_API}/audiences/v1/page`, (route) =>
    json(route, pagedResult(AUDIENCES)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/audiences/v1/issuer/[0-9a-f-]+$`),
    (route) => json(route, AUDIENCES.filter((a) => a.issuerId === ISSUERS[0].id)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/audiences/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, AUDIENCES[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, AUDIENCES[0]);
  });
  await page.route(`${ADMIN_API}/audiences/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Roles
  await page.route(`${ADMIN_API}/roles/v1/page`, (route) =>
    json(route, pagedResult(ROLES)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/roles/v1/audience/[0-9a-f-]+$`),
    (route) => json(route, ROLES.filter((r) => r.audienceId === AUDIENCES[0].id)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/roles/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, ROLES[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, ROLES[0]);
  });
  await page.route(`${ADMIN_API}/roles/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Users
  await page.route(`${ADMIN_API}/users/v1/page`, (route) =>
    json(route, pagedResult(USERS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/users/v1/[0-9a-f-]+/roles$`), (route) =>
    json(route, [ROLES[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/users/v1/[0-9a-f-]+/claims$`), (route) =>
    json(route, [CLAIMS[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/users/v1/[0-9a-f-]+/login-providers$`), (route) =>
    json(route, [LOGIN_PROVIDERS[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/users/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, USERS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, USERS[0]);
  });
  await page.route(`${ADMIN_API}/users/v1/password`, (route) => ok(route));
  await page.route(`${ADMIN_API}/users/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Claims
  await page.route(`${ADMIN_API}/claims/v1/page`, (route) =>
    json(route, pagedResult(CLAIMS)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/claims/v1/issuer/[0-9a-f-]+$`),
    (route) => json(route, CLAIMS),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/claims/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, CLAIMS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, CLAIMS[0]);
  });
  await page.route(`${ADMIN_API}/claims/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Login Providers
  await page.route(`${ADMIN_API}/login-providers/v1/page`, (route) =>
    json(route, pagedResult(LOGIN_PROVIDERS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/login-providers/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, LOGIN_PROVIDERS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, LOGIN_PROVIDERS[0]);
  });
  await page.route(`${ADMIN_API}/login-providers/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Auth Activity
  await page.route(`${ADMIN_API}/activities/v1/page`, (route) =>
    json(route, pagedResult(USER_ACTIVITIES)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/activities/v1/[0-9a-f-]+$`), (route) =>
    json(route, USER_ACTIVITIES[0]),
  );

  // Quotes
  await page.route(`${ADMIN_API}/quotes/v1/page`, (route) =>
    json(route, pagedResult(QUOTES)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/quotes/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, QUOTES[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, QUOTES[0]);
  });
  await page.route(`${ADMIN_API}/quotes/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { globalId: crypto.randomUUID(), ...body });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Email Queue (Enqueue/Dequeue)
  await page.route(`${ADMIN_API}/enqueue/v1/email/page`, (route) =>
    json(route, pagedResult(EMAIL_QUEUE)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/dequeue/v1/email/[0-9a-f-]+$`), (route) =>
    ok(route),
  );

  // Text Queue (Enqueue/Dequeue)
  await page.route(`${ADMIN_API}/enqueue/v1/text/page`, (route) =>
    json(route, pagedResult(TEXT_QUEUE)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/dequeue/v1/text/[0-9a-f-]+$`), (route) =>
    ok(route),
  );

  // Entitlements (shared lookups)
  await page.route(`${ADMIN_API}/entitlements/v1/types`, (route) =>
    json(route, [
      { id: 'tttt0001-0001-0001-0001-000000000001', name: 'Admin', sortOrder: 1, isEnabled: true, isDeletable: false },
      { id: 'tttt0001-0001-0001-0001-000000000002', name: 'User', sortOrder: 2, isEnabled: true, isDeletable: false },
      { id: 'tttt0001-0001-0001-0001-000000000003', name: 'Viewer', sortOrder: 3, isEnabled: true, isDeletable: false },
    ]),
  );
  await page.route(`${ADMIN_API}/entitlements/v1/scopes`, (route) =>
    json(route, [
      { id: 'ssss0001-0001-0001-0001-000000000001', name: 'Global', sortOrder: 1, isEnabled: true },
      { id: 'ssss0001-0001-0001-0001-000000000002', name: 'Issuer', sortOrder: 2, isEnabled: true },
      { id: 'ssss0001-0001-0001-0001-000000000003', name: 'Audience', sortOrder: 3, isEnabled: true },
    ]),
  );

  // User Entitlements
  await page.route(`${ADMIN_API}/entitlements/v1/users/me`, (route) =>
    json(route, ENTITLEMENTS.filter(e => e.userName === 'admin@local')),
  );
  await page.route(`${ADMIN_API}/entitlements/v1/users/page`, (route) =>
    json(route, pagedResult(ENTITLEMENTS)),
  );

  // Audience Entitlements
  await page.route(`${ADMIN_API}/entitlements/v1/audiences/page`, (route) =>
    json(route, pagedResult(AUDIENCE_ENTITLEMENTS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/entitlements/v1/audiences/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, AUDIENCE_ENTITLEMENTS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, AUDIENCE_ENTITLEMENTS[0]);
  });
  await page.route(`${ADMIN_API}/entitlements/v1/audiences`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, created: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Jobs
  await page.route(`${ADMIN_API}/jobs/v1`, (route) => {
    const method = route.request().method();
    if (method === 'GET') return json(route, JOBS);
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/jobs/v1/[0-9a-f-]+/settings$`), (route) =>
    ok(route),
  );

  // LLM Providers
  await page.route(`${ADMIN_API}/llm-providers/v1/order`, (route) => ok(route));
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/llm-providers/v1/[0-9a-f-]+/settings$`), (route) =>
    ok(route),
  );
  await page.route(`${ADMIN_API}/llm-providers/v1`, (route) => {
    const method = route.request().method();
    if (method === 'GET') return json(route, LLM_PROVIDERS);
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Chat API — favorites
  await page.route(`${ADMIN_API}/chat/favorites`, (route) => {
    if (route.request().method() === 'GET') return json(route, CHAT_FAVORITES);
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        name: body.name,
        prompt: body.prompt,
        pinned: false,
        created: new Date().toISOString(),
      });
    }
    return route.fallback();
  });
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/chat/favorites/[0-9a-f-]+$`),
    (route) => {
      if (route.request().method() === 'PUT') {
        const body = route.request().postDataJSON();
        return json(route, { ...body, id: route.request().url().split('/').pop(), created: new Date().toISOString() });
      }
      if (route.request().method() === 'DELETE') return ok(route);
      return route.fallback();
    },
  );

  // Chat API — prompt history
  await page.route(`${ADMIN_API}/chat/prompt-history`, (route) => {
    if (route.request().method() === 'GET') return json(route, PROMPT_HISTORY);
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        promptText: body.promptText,
        created: new Date().toISOString(),
      });
    }
    return route.fallback();
  });

  // Chat API — conversations REST endpoint (used by ChatComponent.ngOnInit)
  await page.route(`${ADMIN_API}/chat/**`, (route) => {
    if (route.request().method() === 'GET') return json(route, []);
    if (route.request().method() === 'POST') {
      return json(route, {
        id: crypto.randomUUID(),
        title: 'New Chat',
        created: new Date().toISOString(),
        updatedUtc: new Date().toISOString(),
      });
    }
    if (route.request().method() === 'DELETE') return ok(route);
    return route.fallback();
  });

  // SignalR hub — fail negotiate fast so the connection settles into disconnected state
  // Tests that need specific hub behavior (e.g. blocking negotiate) override this route
  await page.route(`${ADMIN_API}/hubs/**`, (route) =>
    route.fulfill({ status: 503, body: 'Service Unavailable' }),
  );
}

// ── User API Mocks ───────────────────────────────────────────────────

/** Mock all user-portal API routes */
export async function mockUserApi(page: Page) {
  // Profile
  await page.route(`${USER_API}/profiles/v1`, (route) => {
    if (route.request().method() === 'GET') return json(route, USERS[1]);
    if (route.request().method() === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Sessions
  await page.route(`${USER_API}/sessions/v1/refreshes`, (route) => {
    if (route.request().method() === 'GET') return json(route, REFRESHES);
    if (route.request().method() === 'DELETE') return ok(route);
    return route.fallback();
  });
  await page.route(
    new RegExp(`${escapeRegex(USER_API)}/sessions/v1/refreshes/[0-9a-f-]+$`),
    (route) => ok(route),
  );

  // Logout
  await page.route(`${USER_API}/sessions/v1/logout`, (route) => ok(route));

  // Quote
  await page.route(`${USER_API}/quotes/v1/page`, (route) => json(route, pagedResult(QUOTES)));
  await page.route(`${USER_API}/quotes/v1`, (route) => json(route, QUOTE));

  // Password / Credentials
  await page.route(`${USER_API}/credentials/v1/password`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/password/set`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/email/confirm`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/phone/confirm`, (route) => ok(route));

  // Chat API — favorites
  await page.route(`${USER_API}/chat/favorites`, (route) => {
    if (route.request().method() === 'GET') return json(route, CHAT_FAVORITES);
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        name: body.name,
        prompt: body.prompt,
        pinned: false,
        created: new Date().toISOString(),
      });
    }
    return route.fallback();
  });
  await page.route(
    new RegExp(`${escapeRegex(USER_API)}/chat/favorites/[0-9a-f-]+$`),
    (route) => {
      if (route.request().method() === 'PUT') {
        const body = route.request().postDataJSON();
        return json(route, { ...body, id: route.request().url().split('/').pop(), created: new Date().toISOString() });
      }
      if (route.request().method() === 'DELETE') return ok(route);
      return route.fallback();
    },
  );

  // Chat API — prompt history
  await page.route(`${USER_API}/chat/prompt-history`, (route) => {
    if (route.request().method() === 'GET') return json(route, PROMPT_HISTORY);
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        promptText: body.promptText,
        created: new Date().toISOString(),
      });
    }
    return route.fallback();
  });

  // Chat API — conversations REST endpoint
  await page.route(`${USER_API}/chat/**`, (route) => {
    if (route.request().method() === 'GET') return json(route, []);
    if (route.request().method() === 'POST') {
      return json(route, {
        id: crypto.randomUUID(),
        title: 'New Chat',
        created: new Date().toISOString(),
        updatedUtc: new Date().toISOString(),
      });
    }
    if (route.request().method() === 'DELETE') return ok(route);
    return route.fallback();
  });

  // SignalR hub — fail negotiate fast so the connection settles into disconnected state
  await page.route(`${USER_API}/hubs/**`, (route) =>
    route.fulfill({ status: 503, body: 'Service Unavailable' }),
  );
}

// ── JWT Token Helper ─────────────────────────────────────────────────

/**
 * Create a mock JWT token (unsigned). The browser doesn't verify
 * the signature, it just base64-decodes the payload for claims.
 */
export function createMockJwt(
  email: string,
  roles: string[] = [],
  options?: { sub?: string; issuer?: string; expiresInSec?: number },
): string {
  const sub = options?.sub ?? (email === 'admin@local' ? USERS[0].id : USERS[1].id);
  const issuer = options?.issuer ?? 'Bhbk';
  const now = Math.floor(Date.now() / 1000);
  const exp = now + (options?.expiresInSec ?? 3600);

  // Default roles based on user
  const effectiveRoles =
    roles.length > 0
      ? roles
      : email === 'admin@local'
        ? ['Identity.Admins', 'Identity.Users']
        : ['Identity.Users'];

  const header = { alg: 'none', typ: 'JWT' };
  const payload = {
    sub,
    iss: issuer,
    aud: ['identity-admin', 'identity-user'],
    exp,
    iat: now,
    nbf: now,
    jti: crypto.randomUUID(),
    email,
    name: email === 'admin@local' ? 'Admin User' : 'Regular User',
    given_name: email === 'admin@local' ? 'Admin' : 'Regular',
    family_name: 'User',
    // .NET serializes ClaimTypes.Role as 'role' (singular)
    role: effectiveRoles.length === 1 ? effectiveRoles[0] : effectiveRoles,
  };

  const encode = (obj: unknown) =>
    Buffer.from(JSON.stringify(obj)).toString('base64url');

  return `${encode(header)}.${encode(payload)}.mock-signature`;
}

// ── Utilities ────────────────────────────────────────────────────────

function escapeRegex(str: string): string {
  return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
