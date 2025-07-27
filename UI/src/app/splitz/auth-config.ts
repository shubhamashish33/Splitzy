// auth-config.ts
import { SocialAuthServiceConfig, GoogleLoginProvider } from '@abacritt/angularx-social-login';

export function provideSocialAuthConfig(): SocialAuthServiceConfig {
    return {
        autoLogin: false,
        providers: [
            {
                id: GoogleLoginProvider.PROVIDER_ID,
                provider: new GoogleLoginProvider('181191664943-7an6r5qqerlrp94gdenbv0scso4id5qf.apps.googleusercontent.com')
            }
        ],
        onError: (err) => {
            console.error('Social login error', err);
        }
    };
}
