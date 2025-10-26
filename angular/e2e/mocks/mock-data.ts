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
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '11111111-1111-1111-1111-222222222222',
    name: 'TestIssuer',
    description: 'Test issuer',
    isEnabled: true,
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
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
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    issuerId: ISSUERS[0].id,
    name: 'identity-user',
    description: 'User portal audience',
    isLockedOut: false,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '22222222-2222-2222-2222-333333333333',
    issuerId: ISSUERS[1].id,
    name: 'test-audience',
    description: 'Test audience',
    isLockedOut: false,
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
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
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '33333333-3333-3333-3333-222222222222',
    audienceId: AUDIENCES[1].id,
    name: 'Identity.Users',
    description: 'Standard user role',
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    audienceId: AUDIENCES[2].id,
    name: 'Test.Role',
    description: 'Test role',
    isEnabled: true,
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
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
    created: '2024-01-01T00:00:00Z',
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
    created: '2024-01-02T00:00:00Z',
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
    created: '2024-03-01T00:00:00Z',
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
    created: '2024-03-15T00:00:00Z',
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
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '55555555-5555-5555-5555-222222222222',
    issuerId: ISSUERS[0].id,
    type: 'role',
    value: 'Identity.Admins',
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '55555555-5555-5555-5555-333333333333',
    issuerId: ISSUERS[1].id,
    type: 'scope',
    value: 'read',
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
  },
];

// ── Entitlements ────────────────────────────────────────────────────

export const ENTITLEMENTS = [
  {
    id: 'eeee0001-0001-0001-0001-000000000001',
    userId: USERS[0].id,
    entitlementTypeId: 'tttt0001-0001-0001-0001-000000000001',
    entitlementScopeId: 'ssss0001-0001-0001-0001-000000000001',
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
    userName: 'admin@local',
    entitlementTypeName: 'Admin',
    entitlementScopeName: 'Global',
  },
  {
    id: 'eeee0001-0001-0001-0001-000000000002',
    userId: USERS[1].id,
    entitlementTypeId: 'tttt0001-0001-0001-0001-000000000002',
    entitlementScopeId: 'ssss0001-0001-0001-0001-000000000001',
    isEnabled: true,
    isDeletable: true,
    created: '2024-01-02T00:00:00Z',
    userName: 'user@local',
    entitlementTypeName: 'User',
    entitlementScopeName: 'Global',
  },
];

// ── Audience Entitlements ─────────────────────────────────────────────

export const AUDIENCE_ENTITLEMENTS = [
  {
    id: 'aeae0001-0001-0001-0001-000000000001',
    audienceId: AUDIENCES[0].id,
    entitlementTypeId: 'tttt0001-0001-0001-0001-000000000001',
    entitlementScopeId: 'ssss0001-0001-0001-0001-000000000001',
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
    audienceName: 'identity-admin',
    entitlementTypeName: 'Admin',
    entitlementScopeName: 'Global',
  },
];

// ── Login Providers ──────────────────────────────────────────────────

export const LOGIN_PROVIDERS = [
  {
    id: '66666666-6666-6666-6666-111111111111',
    name: 'Local',
    description: 'Local password provider',
    providerKey: 'local-provider',
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
  },
  {
    id: '66666666-6666-6666-6666-222222222222',
    name: 'Google',
    description: 'Google OAuth provider',
    providerKey: 'google-oauth',
    isEnabled: true,
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
  },
];

// ── Activity ─────────────────────────────────────────────────────────

export const USER_ACTIVITIES = [
  {
    id: '77777777-7777-7777-7777-111111111111',
    audienceIds: [AUDIENCES[0].id],
    userId: USERS[0].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Success',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '127.0.0.1:54321',
    created: '2024-06-15T10:30:00Z',
  },
  {
    id: '77777777-7777-7777-7777-222222222222',
    audienceIds: [AUDIENCES[0].id, AUDIENCES[1].id],
    userId: USERS[1].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Success',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '127.0.0.1:54322',
    created: '2024-06-15T11:00:00Z',
  },
  {
    id: '77777777-7777-7777-7777-333333333333',
    audienceIds: [],
    userId: USERS[1].id,
    loginType: 'ResourceOwner',
    loginOutcome: 'Failure',
    localEndpoint: '127.0.0.1:55114',
    remoteEndpoint: '192.168.1.50:12345',
    created: '2024-06-15T09:00:00Z',
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
    validFrom: '2024-06-15T10:30:00Z',
    validTo: '2024-06-22T10:30:00Z',
    issued: '2024-06-15T10:30:00Z',
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
    validFrom: '2024-06-15T11:00:00Z',
    validTo: '2024-06-22T11:00:00Z',
    issued: '2024-06-15T11:00:00Z',
    ipAddress: '192.168.1.10',
    userAgent: 'Mozilla/5.0 Firefox',
    deviceName: 'Laptop Firefox',
  },
];

// ── Quotes ──────────────────────────────────────────────────────────

export const QUOTE = {
  globalId: '99999999-9999-9999-9999-111111111111',
  author: 'System',
  quote: 'Welcome to the Identity Portal. Have a productive day!',
  tags: ['welcome'],
  category: 'general',
};

export const QUOTES = [
  QUOTE,
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

// ── Email Queue ─────────────────────────────────────────────────────

export const EMAIL_QUEUE = [
  {
    id: 'aaaa1111-1111-1111-1111-111111111111',
    fromEmail: 'noreply@identity.local',
    fromDisplay: 'Identity System',
    toEmail: 'admin@local',
    toDisplay: 'Admin User',
    subject: 'Email Confirmation',
    body: '<p>Please confirm your email address by clicking the link below.</p>',
    isCancelled: false,
    created: '2024-06-15T08:00:00Z',
    sendAt: '2024-06-15T08:05:00Z',
    delivered: '2024-06-15T08:05:12Z',
  },
  {
    id: 'aaaa1111-1111-1111-1111-222222222222',
    fromEmail: 'noreply@identity.local',
    fromDisplay: 'Identity System',
    toEmail: 'user@local',
    toDisplay: 'Regular User',
    subject: 'Password Reset',
    body: '<p>You requested a password reset. Use the code below.</p>',
    isCancelled: false,
    created: '2024-06-15T09:00:00Z',
    sendAt: '2024-06-15T09:05:00Z',
    delivered: null,
  },
  {
    id: 'aaaa1111-1111-1111-1111-333333333333',
    fromEmail: 'noreply@identity.local',
    fromDisplay: 'Identity System',
    toEmail: 'locked@local',
    toDisplay: 'Locked Account',
    subject: 'Account Locked',
    body: '<p>Your account has been locked due to too many failed attempts.</p>',
    isCancelled: true,
    created: '2024-06-15T10:00:00Z',
    sendAt: '2024-06-15T10:05:00Z',
    delivered: null,
  },
];

// ── Text Queue ──────────────────────────────────────────────────────

export const TEXT_QUEUE = [
  {
    id: 'bbbb2222-2222-2222-2222-111111111111',
    fromPhoneNumber: '+15551234567',
    toPhoneNumber: '+15559876543',
    body: 'Your verification code is 482910. It expires in 10 minutes.',
    isCancelled: false,
    created: '2024-06-15T08:30:00Z',
    sendAt: '2024-06-15T08:30:00Z',
    delivered: '2024-06-15T08:30:05Z',
  },
  {
    id: 'bbbb2222-2222-2222-2222-222222222222',
    fromPhoneNumber: '+15551234567',
    toPhoneNumber: '+15558765432',
    body: 'Your verification code is 193847. It expires in 10 minutes.',
    isCancelled: false,
    created: '2024-06-15T09:15:00Z',
    sendAt: '2024-06-15T09:15:00Z',
    delivered: null,
  },
];

// ── Jobs ────────────────────────────────────────────────────────────

export const JOBS = [
  {
    id: 'cccc0001-0001-0001-0001-000000000001',
    name: 'Maintain Quotes',
    description: 'Fetches and rotates daily quotes',
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
    settings: [
      {
        id: 'dddd0001-0001-0001-0001-000000000001',
        jobId: 'cccc0001-0001-0001-0001-000000000001',
        configKey: 'Schedule',
        configValue: '0 0 6 * * ?',
        isSecret: false,
      },
      {
        id: 'dddd0001-0001-0001-0001-000000000002',
        jobId: 'cccc0001-0001-0001-0001-000000000001',
        configKey: 'ApiKey',
        configValue: '********',
        isSecret: true,
      },
    ],
  },
  {
    id: 'cccc0001-0001-0001-0001-000000000002',
    name: 'Groom Chat History',
    description: 'Removes stale chat conversations',
    isEnabled: false,
    isDeletable: false,
    created: '2024-02-01T00:00:00Z',
    settings: [
      {
        id: 'dddd0001-0001-0001-0001-000000000003',
        jobId: 'cccc0001-0001-0001-0001-000000000002',
        configKey: 'Schedule',
        configValue: '0 0 2 * * ?',
        isSecret: false,
      },
      {
        id: 'dddd0001-0001-0001-0001-000000000004',
        jobId: 'cccc0001-0001-0001-0001-000000000002',
        configKey: 'MaxAgeDays',
        configValue: '90',
        isSecret: false,
      },
    ],
  },
];

// ── LLM Providers ──────────────────────────────────────────────────

export const LLM_PROVIDERS = [
  {
    id: 'ffff0001-0001-0001-0001-000000000001',
    name: 'Ollama Local',
    failoverOrder: 1,
    isEnabled: true,
    isDeletable: false,
    created: '2024-01-01T00:00:00Z',
    settings: [
      {
        id: 'gggg0001-0001-0001-0001-000000000001',
        llmProviderId: 'ffff0001-0001-0001-0001-000000000001',
        configKey: 'BaseUrl',
        configValue: 'http://localhost:11434',
        isSecret: false,
      },
      {
        id: 'gggg0001-0001-0001-0001-000000000002',
        llmProviderId: 'ffff0001-0001-0001-0001-000000000001',
        configKey: 'ModelName',
        configValue: 'llama3',
        isSecret: false,
      },
    ],
  },
  {
    id: 'ffff0001-0001-0001-0001-000000000002',
    name: 'AWS Bedrock',
    failoverOrder: 2,
    isEnabled: false,
    isDeletable: true,
    created: '2024-02-01T00:00:00Z',
    settings: [
      {
        id: 'gggg0001-0001-0001-0001-000000000003',
        llmProviderId: 'ffff0001-0001-0001-0001-000000000002',
        configKey: 'Region',
        configValue: 'us-east-1',
        isSecret: false,
      },
      {
        id: 'gggg0001-0001-0001-0001-000000000004',
        llmProviderId: 'ffff0001-0001-0001-0001-000000000002',
        configKey: 'AccessKey',
        configValue: '********',
        isSecret: true,
      },
    ],
  },
];

// ── Prompt History ─────────────────────────────────────────────────

export const PROMPT_HISTORY = [
  {
    id: 'hhhh0001-0001-0001-0001-000000000001',
    promptText: 'Show me all active users',
    created: '2026-02-27T10:00:00Z',
  },
  {
    id: 'hhhh0001-0001-0001-0001-000000000002',
    promptText: 'What roles does admin@local have?',
    created: '2026-02-27T10:05:00Z',
  },
  {
    id: 'hhhh0001-0001-0001-0001-000000000003',
    promptText: 'List all issuers',
    created: '2026-02-27T10:10:00Z',
  },
];

// ── Chat Favorites ────────────────────────────────────────────────

export const CHAT_FAVORITES = [
  {
    id: 'ffff0001-0001-0001-0001-000000000001',
    name: 'User count',
    prompt: 'How many users are in the system?',
    pinned: false,
    created: '2026-02-27T10:00:00Z',
  },
  {
    id: 'ffff0001-0001-0001-0001-000000000002',
    name: 'Recent activity',
    prompt: 'Show recent authentication activity',
    pinned: true,
    created: '2026-02-27T10:01:00Z',
  },
  {
    id: 'ffff0001-0001-0001-0001-000000000003',
    name: 'System roles',
    prompt: 'What roles exist in the system?',
    pinned: false,
    created: '2026-02-27T10:02:00Z',
  },
  {
    id: 'ffff0001-0001-0001-0001-000000000004',
    name: 'List audiences',
    prompt: 'List all audiences',
    pinned: false,
    created: '2026-02-27T10:03:00Z',
  },
];

// ── Helpers ──────────────────────────────────────────────────────────

/** Wrap an array in a PagedResult shape */
export function pagedResult<T>(data: T[], total?: number) {
  return { data, total: total ?? data.length };
}
