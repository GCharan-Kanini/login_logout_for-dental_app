import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService } from './auth/auth.service';
import { LogoutControlComponent } from './auth/logout-control.component';
import { SignInComponent } from './auth/sign-in.component';

@Component({
  selector: 'app-root',
  imports: [LogoutControlComponent, SignInComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  protected readonly auth = inject(AuthService);
}
