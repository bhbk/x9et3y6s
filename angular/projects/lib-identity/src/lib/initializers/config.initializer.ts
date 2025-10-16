import { APP_INITIALIZER, EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { ConfigService } from '../services/config.service';

function initializeConfig(configService: ConfigService): () => Promise<void> {
  return () => configService.loadConfig();
}

export function provideAppConfig(): EnvironmentProviders {
  return makeEnvironmentProviders([
    {
      provide: APP_INITIALIZER,
      useFactory: initializeConfig,
      deps: [ConfigService],
      multi: true
    }
  ]);
}
