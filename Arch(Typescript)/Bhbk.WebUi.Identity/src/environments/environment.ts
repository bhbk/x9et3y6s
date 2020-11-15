// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  issuer: "Bhbk",
  "IdentityAdminUrls": {
    "BaseApiUrl": "https://localhost:44359",
    "BaseApiPath": "/api/identity/admin",
    "BaseUiUrl": "http://localhost:4300",
    "BaseUiPath": "/ui/identity/admin"
  },
  "IdentityMeUrls": {
    "BaseApiUrl": "https://localhost:44348",
    "BaseApiPath": "/api/identity/me",
    "BaseUiUrl": "http://localhost:4300",
    "BaseUiPath": "/ui/identity/me"
  },
  "IdentityStsUrls": {
    "BaseApiUrl": "https://localhost:44375",
    "BaseApiPath": "/api/identity/sts"
  },
  tokenName: "currentUser"
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
