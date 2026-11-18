import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AuthService } from './auth/auth.service';
import { AppComponent } from './app.component';

describe('AppComponent logout', () => {
  let fixture: ComponentFixture<AppComponent>;
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    fixture = TestBed.createComponent(AppComponent);
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function clickLogout(): void {
    (element().querySelector('button') as HTMLButtonElement).click();
    fixture.detectChanges();
  }

  it('shows the sign-in view when no session is active', () => {
    fixture.detectChanges();

    expect(element().querySelector('app-sign-in')).not.toBeNull();
    expect(element().querySelector('app-logout-control')).toBeNull();
  });

  it('AC-1: after the server confirms logout, the sign-in view is shown and the session is gone', () => {
    auth.establishSession('session-1');
    fixture.detectChanges();

    clickLogout();
    http
      .expectOne('/api/auth/logout')
      .flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(auth.isSignedIn()).toBeFalse();
    expect(element().querySelector('app-sign-in')).not.toBeNull();
    expect(element().querySelector('app-logout-control')).toBeNull();
  });

  it('AC-2: after logout the signed-out UI offers no control that could reuse the old session', () => {
    auth.establishSession('session-1');
    fixture.detectChanges();
    clickLogout();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(element().querySelector('button')).toBeNull();
    expect(element().querySelector('app-sign-in')).not.toBeNull();
  });

  it('shows an error and keeps the logout control when the server fails to end the session', () => {
    auth.establishSession('session-1');
    fixture.detectChanges();

    clickLogout();
    http
      .expectOne('/api/auth/logout')
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();

    expect(element().querySelector('[role="alert"]')?.textContent).toContain('Logout failed');
    expect(element().querySelector('app-logout-control')).not.toBeNull();
    expect(element().querySelector('app-sign-in')).toBeNull();
  });

  it('AC-3: after a failed logout, a successful retry shows the sign-in view and no authenticated session remains', () => {
    auth.establishSession('session-1');
    fixture.detectChanges();

    clickLogout();
    http.expectOne('/api/auth/logout').flush(null, { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();

    const retry = element().querySelector('.logout__error button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(auth.isSignedIn()).toBeFalse();
    expect(element().querySelector('app-sign-in')).not.toBeNull();
    expect(element().querySelector('app-logout-control')).toBeNull();
  });
});
