import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  // Held in memory only. Nothing is written to local or session storage or to cookies, so clearing the
  // signal is the whole of clearing client-side credentials.
  private readonly sessionId = signal<string | null>(null);

  readonly isSignedIn = computed(() => this.sessionId() !== null);
  readonly loggingOut = signal(false);
  readonly logoutFailed = signal(false);

  // Called by the sign-in flow once the server has issued a session. The sign-in story wires this up.
  establishSession(sessionId: string): void {
    this.sessionId.set(sessionId);
    this.logoutFailed.set(false);
  }

  // Headers for a request made as the signed-in staff member, or null when no session is active.
  authorizationHeaders(): { Authorization: string } | null {
    const sessionId = this.sessionId();
    return sessionId === null ? null : { Authorization: `Bearer ${sessionId}` };
  }

  // Also used by the retry action: a failed logout leaves the session in place, so logout can be sent again.
  logout(): void {
    const headers = this.authorizationHeaders();
    if (headers === null || this.loggingOut()) {
      return;
    }

    this.loggingOut.set(true);
    this.logoutFailed.set(false);

    this.http
      .post('/api/auth/logout', null, { headers, observe: 'response' })
      .pipe(finalize(() => this.loggingOut.set(false)))
      .subscribe({
        next: (response) => this.onLogoutResponse(response),
        error: () => this.logoutFailed.set(true),
      });
  }

  private onLogoutResponse(response: HttpResponse<unknown>): void {
    // Only the server's confirmation ends the local session. Any other answer leaves the user signed in.
    if (response.status === 204) {
      this.sessionId.set(null);
      this.logoutFailed.set(false);
      return;
    }

    this.logoutFailed.set(true);
  }
}
