import { environment } from '../../../environments/environment';

// Base URL for the ASP.NET Core backend. Comes from environment.ts locally
// (localhost) and environment.production.ts when built with
// `ng build --configuration production` (the real deployed API URL).
export const API_BASE_URL = environment.apiUrl;
