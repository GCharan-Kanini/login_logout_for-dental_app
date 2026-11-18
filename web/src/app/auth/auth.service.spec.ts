import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    service.establishSession('session-1');
  });

  afterEach(() => http.verify());

  it('posts to the logout endpoint with the session as a bearer token', () => {
    service.logout();

    const request = http.expectOne('/api/auth/logout');
    expect(request.request.method).toBe('POST');
    expect(request.request.headers.get('Authorization')).toBe('Bearer session-1');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('clears the session when the server confirms the logout with 204', () => {
    service.logout();

    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });

    expect(service.isSignedIn()).toBeFalse();
    expect(service.logoutFailed()).toBeFalse();
    expect(service.loggingOut()).toBeFalse();
  });

  it('keeps the session and reports a failure when the server answers with an error', () => {
    service.logout();

    http
      .expectOne('/api/auth/logout')
      .flush(null, { status: 503, statusText: 'Service Unavailable' });

    expect(service.isSignedIn()).toBeTrue();
    expect(service.logoutFailed()).toBeTrue();
    expect(service.loggingOut()).toBeFalse();
  });

  it('keeps the session and reports a failure on a network error', () => {
    service.logout();

    http.expectOne('/api/auth/logout').error(new ProgressEvent('error'));

    expect(service.isSignedIn()).toBeTrue();
    expect(service.logoutFailed()).toBeTrue();
    expect(service.loggingOut()).toBeFalse();
  });

  it('marks the logout as pending until the server answers', () => {
    service.logout();
    expect(service.loggingOut()).toBeTrue();

    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });
    expect(service.loggingOut()).toBeFalse();
  });

  it('does not send a second logout while one is pending', () => {
    service.logout();
    service.logout();

    const request = http.expectOne('/api/auth/logout');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('sends no request when no session is active', () => {
    service.logout();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });

    service.logout();

    http.expectNone('/api/auth/logout');
  });

  it('keeps the session usable after a failed logout', () => {
    service.logout();
    http
      .expectOne('/api/auth/logout')
      .flush(null, { status: 503, statusText: 'Service Unavailable' });

    const headers = service.authorizationHeaders();
    expect(headers).toEqual({ Authorization: 'Bearer session-1' });

    let status: number | undefined;
    TestBed.inject(HttpClient)
      .get('/api/staff/me', { headers: headers ?? {}, observe: 'response' })
      .subscribe((response) => {
        status = response.status;
      });
    http.expectOne('/api/staff/me').flush({ user_id: 'user-42' });

    expect(status).toBe(200);
  });

  it('has no authorization headers once the logout is confirmed', () => {
    service.logout();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });

    expect(service.authorizationHeaders()).toBeNull();
  });
});
