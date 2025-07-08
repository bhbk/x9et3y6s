/**
 * Canned API response data for Playwright route mocks.
 * Shapes match the .NET API response contracts (camelCase via CamelCasePropertyNamesContractResolver).
 */

// ── Issuers ──────────────────────────────────────────────────────────

export const ISSUERS = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    name: 'Bhbk',
    description: 'Primary issuer',
    isEnabled: true,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '11111111-1111-1111-1111-222222222222',
    name: 'TestIssuer',
    description: 'Test issuer',
    isEnabled: true,
    isDeletable: true,
    createdUtc: '2024-02-01T00:00:00Z',
  },
];

// ── Audiences ────────────────────────────────────────────────────────

export const AUDIENCES = [
  {
    id: '22222222-2222-2222-2222-111111111111',
    issuerId: ISSUERS[0].id,
    name: 'identity-admin',
    description: 'Admin portal audience',
    isLockedOut: false,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    issuerId: ISSUERS[0].id,
    name: 'identity-user',
    description: 'User portal audience',
    isLockedOut: false,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '22222222-2222-2222-2222-333333333333',
    issuerId: ISSUERS[1].id,
    name: 'test-audience',
    description: 'Test audience',
    isLockedOut: false,
    isDeletable: true,
    createdUtc: '2024-02-01T00:00:00Z',
  },
];

// ── Roles ────────────────────────────────────────────────────────────

export const ROLES = [
  {
    id: '33333333-3333-3333-3333-111111111111',
    audienceId: AUDIENCES[0].id,
    name: 'Identity.Admins',
    description: 'Administrator role',
    isEnabled: true,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '33333333-3333-3333-3333-222222222222',
    audienceId: AUDIENCES[1].id,
    name: 'Identity.Users',
    description: 'Standard user role',
    isEnabled: true,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    audienceId: AUDIENCES[2].id,
    name: 'Test.Role',
    description: 'Test role',
    isEnabled: true,
    isDeletable: true,
    createdUtc: '2024-02-01T00:00:00Z',
  },
];

// ── Users ────────────────────────────────────────────────────────────

export const USERS = [
  {
    id: '44444444-4444-4444-4444-111111111111',
    userName: 'admin@local',
    email: 'admin@local',
    firstName: 'Admin',
    lastName: 'User',
    isHumanBeing: true,
    isLockedOut: false,
    isDeletable: false,
    emailConfirmed: true,
    passwordConfirmed: true,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '44444444-4444-4444-4444-222222222222',
    userName: 'user@local',
    email: 'user@local',
    firstName: 'Regular',
    lastName: 'User',
    isHumanBeing: true,
    isLockedOut: false,
    isDeletable: true,
    emailConfirmed: true,
    passwordConfirmed: true,
    createdUtc: '2024-01-02T00:00:00Z',
  },
  {
    id: '44444444-4444-4444-4444-333333333333',
    userName: 'locked@local',
    email: 'locked@local',
    firstName: 'Locked',
    lastName: 'Account',
    isHumanBeing: true,
    isLockedOut: true,
    isDeletable: true,
    emailConfirmed: true,
    passwordConfirmed: true,
    createdUtc: '2024-03-01T00:00:00Z',
  },
  {
    id: '44444444-4444-4444-4444-444444444444',
    userName: 'pending@local',
    email: 'pending@local',
    firstName: 'Pending',
    lastName: 'Confirmation',
    isHumanBeing: true,
    isLockedOut: false,
    isDeletable: true,
    emailConfirmed: false,
    passwordConfirmed: false,
    createdUtc: '2024-03-15T00:00:00Z',
  },
];

// ── Claims ───────────────────────────────────────────────────────────

export const CLAIMS = [
  {
    id: '55555555-5555-5555-5555-111111111111',
    issuerId: ISSUERS[0].id,
    type: 'email',
    value: 'admin@local',
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '55555555-5555-5555-5555-222222222222',
    issuerId: ISSUERS[0].id,
    type: 'role',
    value: 'Identity.Admins',
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '55555555-5555-5555-5555-333333333333',
    issuerId: ISSUERS[1].id,
    type: 'scope',
    value: 'read',
    isDeletable: true,
    createdUtc: '2024-02-01T00:00:00Z',
  },
];

// ── Logins ───────────────────────────────────────────────────────────

export const LOGINS = [
  {
    id: '66666666-6666-6666-6666-111111111111',
    name: 'Local',
    description: 'Local password provider',
    loginKey: 'local-provider',
    isEnabled: true,
    isDeletable: false,
    createdUtc: '2024-01-01T00:00:00Z',
  },
  {
    id: '66666666-6666-6666-6666-222222222222',
    name: 'Google',
    description: 'Google OAuth provider',
    loginKey: 'google-oauth',
    isEnabled: true,
    isDeletable: true,
    createdUtc: '2024-02-01T00:00:00Z',
  },
];

// ── Auth Activity ────────────────────────────────────────────────────

export const AUTH_ACTIVITIES = [
  {
    id: '77777777-7777-7777-7777-111111111111',
    audienceId: AUDIENCES[0].id,
    userId: USERS[0].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Success',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '127.0.0.1:54321',
    createdUtc: '2024-06-15T10:30:00Z',
  },
  {
    id: '77777777-7777-7777-7777-222222222222',
    audienceId: AUDIENCES[1].id,
    userId: USERS[1].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Success',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '127.0.0.1:54322',
    createdUtc: '2024-06-15T11:00:00Z',
  },
  {
    id: '77777777-7777-7777-7777-333333333333',
    userId: USERS[1].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Failure',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '192.168.1.50:12345',
    createdUtc: '2024-06-15T09:00:00Z',
  },
];

// ── Refreshes / Sessions ─────────────────────────────────────────────

export const REFRESHES = [
  {
    id: '88888888-8888-8888-8888-111111111111',
    issuerId: ISSUERS[0].id,
    audienceId: AUDIENCES[0].id,
    userId: USERS[0].id,
    refreshType: 'User',
    validFromUtc: '2024-06-15T10:30:00Z',
    validToUtc: '2024-06-22T10:30:00Z',
    issuedUtc: '2024-06-15T10:30:00Z',
    ipAddress: '127.0.0.1',
    userAgent: 'Mozilla/5.0 Playwright',
    deviceName: 'Desktop Chrome',
  },
  {
    id: '88888888-8888-8888-8888-222222222222',
    issuerId: ISSUERS[0].id,
    audienceId: AUDIENCES[1].id,
    userId: USERS[1].id,
    refreshType: 'User',
    validFromUtc: '2024-06-15T11:00:00Z',
    validToUtc: '2024-06-22T11:00:00Z',
    issuedUtc: '2024-06-15T11:00:00Z',
    ipAddress: '192.168.1.10',
    userAgent: 'Mozilla/5.0 Firefox',
    deviceName: 'Laptop Firefox',
  },
];

// ── MOTDs ────────────────────────────────────────────────────────────

export const MOTD = {
  globalId: '99999999-9999-9999-9999-111111111111',
  author: 'System',
  quote: 'Welcome to the Identity Portal. Have a productive day!',
  tags: ['welcome'],
  category: 'general',
};

export const MOTDS = [
  MOTD,
  {
    globalId: '99999999-9999-9999-9999-222222222222',
    author: 'Albert Einstein',
    quote: 'Imagination is more important than knowledge.',
    tags: ['inspiration', 'science'],
    category: 'inspiration',
  },
  {
    globalId: '99999999-9999-9999-9999-333333333333',
    author: 'Winston Churchill',
    quote: 'Success is not final, failure is not fatal: it is the courage to continue that counts.',
    tags: ['perseverance'],
    category: 'motivation',
  },
  {
    globalId: '99999999-9999-9999-9999-444444444444',
    author: 'Steve Jobs',
    quote: 'Stay hungry, stay foolish.',
    tags: ['innovation', 'technology'],
    category: 'technology',
  },
  {
    globalId: '99999999-9999-9999-9999-555555555555',
    author: 'Mahatma Gandhi',
    quote: 'Be the change that you wish to see in the world.',
    tags: ['leadership', 'change'],
    category: 'leadership',
  },
];

// ── Helpers ──────────────────────────────────────────────────────────

/** Wrap an array in a PagedResult shape */
export function pagedResult<T>(data: T[], total?: number) {
  return { data, total: total ?? data.length };
}
