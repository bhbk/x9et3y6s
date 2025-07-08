import { Page, Route } from '@playwright/test';
import {
  ISSUERS,
  AUDIENCES,
  ROLES,
  USERS,
  CLAIMS,
  LOGINS,
  AUTH_ACTIVITIES,
  REFRESHES,
  MOTD,
  MOTDS,
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

/** Helper to respond with empty 200 */
function ok(route: Route) {
  return route.fulfill({ status: 200 });
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
  await page.route(`${ADMIN_API}/issuer/v1/page`, (route) =>
    json(route, pagedResult(ISSUERS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/issuer/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, ISSUERS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, ISSUERS[0]); // PUT
  });
  await page.route(`${ADMIN_API}/issuer/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      // Create — return the posted data merged with generated fields
      const body = route.request().postDataJSON();
      return json(route, {
        id: crypto.randomUUID(),
        ...body,
        createdUtc: new Date().toISOString(),
      });
    }
    if (method === 'PUT') {
      const body = route.request().postDataJSON();
      return json(route, body);
    }
    return route.fallback();
  });

  // Audiences
  await page.route(`${ADMIN_API}/audience/v1/page`, (route) =>
    json(route, pagedResult(AUDIENCES)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/audience/v1/issuer/[0-9a-f-]+$`),
    (route) => json(route, AUDIENCES.filter((a) => a.issuerId === ISSUERS[0].id)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/audience/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, AUDIENCES[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, AUDIENCES[0]);
  });
  await page.route(`${ADMIN_API}/audience/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, createdUtc: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Roles
  await page.route(`${ADMIN_API}/role/v1/page`, (route) =>
    json(route, pagedResult(ROLES)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/role/v1/audience/[0-9a-f-]+$`),
    (route) => json(route, ROLES.filter((r) => r.audienceId === AUDIENCES[0].id)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/role/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, ROLES[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, ROLES[0]);
  });
  await page.route(`${ADMIN_API}/role/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, createdUtc: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Users
  await page.route(`${ADMIN_API}/user/v1/page`, (route) =>
    json(route, pagedResult(USERS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/user/v1/[0-9a-f-]+/roles$`), (route) =>
    json(route, [ROLES[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/user/v1/[0-9a-f-]+/claims$`), (route) =>
    json(route, [CLAIMS[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/user/v1/[0-9a-f-]+/logins$`), (route) =>
    json(route, [LOGINS[0]]),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/user/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, USERS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, USERS[0]);
  });
  await page.route(`${ADMIN_API}/user/v1/password`, (route) => ok(route));
  await page.route(`${ADMIN_API}/user/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, createdUtc: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Claims
  await page.route(`${ADMIN_API}/claim/v1/page`, (route) =>
    json(route, pagedResult(CLAIMS)),
  );
  await page.route(
    new RegExp(`${escapeRegex(ADMIN_API)}/claim/v1/issuer/[0-9a-f-]+$`),
    (route) => json(route, CLAIMS),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/claim/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, CLAIMS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, CLAIMS[0]);
  });
  await page.route(`${ADMIN_API}/claim/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, createdUtc: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Logins
  await page.route(`${ADMIN_API}/login/v1/page`, (route) =>
    json(route, pagedResult(LOGINS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/login/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, LOGINS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, LOGINS[0]);
  });
  await page.route(`${ADMIN_API}/login/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { id: crypto.randomUUID(), ...body, createdUtc: new Date().toISOString() });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Auth Activity
  await page.route(`${ADMIN_API}/activity/v1/page`, (route) =>
    json(route, pagedResult(AUTH_ACTIVITIES)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/activity/v1/[0-9a-f-]+$`), (route) =>
    json(route, AUTH_ACTIVITIES[0]),
  );

  // MOTDs
  await page.route(`${ADMIN_API}/motd/v1/page`, (route) =>
    json(route, pagedResult(MOTDS)),
  );
  await page.route(new RegExp(`${escapeRegex(ADMIN_API)}/motd/v1/[0-9a-f-]+$`), (route) => {
    if (route.request().method() === 'GET') return json(route, MOTDS[0]);
    if (route.request().method() === 'DELETE') return ok(route);
    return json(route, MOTDS[0]);
  });
  await page.route(`${ADMIN_API}/motd/v1`, (route) => {
    const method = route.request().method();
    if (method === 'POST') {
      const body = route.request().postDataJSON();
      return json(route, { globalId: crypto.randomUUID(), ...body });
    }
    if (method === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });
}

// ── User API Mocks ───────────────────────────────────────────────────

/** Mock all user-portal API routes */
export async function mockUserApi(page: Page) {
  // Profile
  await page.route(`${USER_API}/profile/v1`, (route) => {
    if (route.request().method() === 'GET') return json(route, USERS[1]);
    if (route.request().method() === 'PUT') return json(route, route.request().postDataJSON());
    return route.fallback();
  });

  // Sessions
  await page.route(`${USER_API}/session/v1/refreshes`, (route) => {
    if (route.request().method() === 'GET') return json(route, REFRESHES);
    if (route.request().method() === 'DELETE') return ok(route);
    return route.fallback();
  });
  await page.route(
    new RegExp(`${escapeRegex(USER_API)}/session/v1/refreshes/[0-9a-f-]+$`),
    (route) => ok(route),
  );

  // Logout
  await page.route(`${USER_API}/session/v1/logout`, (route) => ok(route));

  // MOTD
  await page.route(`${USER_API}/motd/v1/page`, (route) => json(route, pagedResult(MOTDS)));
  await page.route(`${USER_API}/motd/v1`, (route) => json(route, MOTD));

  // Password / Credentials
  await page.route(`${USER_API}/credentials/v1/password`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/password/set`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/email/confirm`, (route) => ok(route));
  await page.route(`${USER_API}/credentials/v1/phone/confirm`, (route) => ok(route));
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
