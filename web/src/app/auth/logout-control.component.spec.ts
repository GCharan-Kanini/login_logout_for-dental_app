import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { LogoutControlComponent } from './logout-control.component';

describe('LogoutControlComponent', () => {
  let fixture: ComponentFixture<LogoutControlComponent>;
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LogoutControlComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    fixture = TestBed.createComponent(LogoutControlComponent);
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    auth.establishSession('session-1');
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function logoutButton(): HTMLButtonElement {
    return element().querySelector('button') as HTMLButtonElement;
  }

  function alertMessage(): HTMLElement | null {
    return element().querySelector('[role="alert"]');
  }

  function retryButton(): HTMLButtonElement | null {
    return element().querySelector('.logout__error button');
  }

  function clickAndRefresh(button: HTMLButtonElement | null): void {
    button?.click();
    fixture.detectChanges();
  }

  function failLogoutWith(status: number): void {
    clickAndRefresh(logoutButton());
    http.expectOne('/api/auth/logout').flush(null, { status, statusText: 'Error' });
    fixture.detectChanges();
  }

  it('shows no error before a logout has failed', () => {
    expect(alertMessage()).toBeNull();
    expect(retryButton()).toBeNull();
  });

  it('AC-3: a server error shows an inline logout-failed message beside the control with a retry action', () => {
    failLogoutWith(503);

    expect(alertMessage()?.textContent).toContain('Logout failed. You are still signed in.');
    expect(retryButton()?.textContent).toContain('Retry');
    expect(alertMessage()?.parentElement).toBe(retryButton()?.parentElement ?? null);
  });

  it('AC-3: a network error shows the same inline logout-failed message', () => {
    clickAndRefresh(logoutButton());

    http.expectOne('/api/auth/logout').error(new ProgressEvent('error'));
    fixture.detectChanges();

    expect(alertMessage()?.textContent).toContain('Logout failed');
    expect(retryButton()).not.toBeNull();
  });

  it('AC-3: a failed logout leaves the client signed in with its session headers intact', () => {
    failLogoutWith(500);

    expect(auth.isSignedIn()).toBeTrue();
    expect(auth.authorizationHeaders()).toEqual({ Authorization: 'Bearer session-1' });
  });

  it('AC-3: retry re-sends the logout with the same session and shows an attempting indicator', () => {
    failLogoutWith(503);

    clickAndRefresh(retryButton());

    const request = http.expectOne('/api/auth/logout');
    expect(request.request.headers.get('Authorization')).toBe('Bearer session-1');
    expect(alertMessage()).toBeNull();
    expect(retryButton()?.textContent).toContain('Retrying');
    expect(retryButton()?.disabled).toBeTrue();
    expect(logoutButton().disabled).toBeTrue();

    request.flush(null, { status: 503, statusText: 'Error' });
  });

  it('AC-3: a retry that fails again shows the error again and keeps the client signed in', () => {
    failLogoutWith(503);

    clickAndRefresh(retryButton());
    http.expectOne('/api/auth/logout').flush(null, { status: 503, statusText: 'Error' });
    fixture.detectChanges();

    expect(alertMessage()?.textContent).toContain('Logout failed');
    expect(auth.isSignedIn()).toBeTrue();
  });

  it('AC-3: a retry that succeeds clears the client session and the error', () => {
    failLogoutWith(503);

    clickAndRefresh(retryButton());
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(auth.isSignedIn()).toBeFalse();
    expect(auth.authorizationHeaders()).toBeNull();
    expect(alertMessage()).toBeNull();
  });
});
