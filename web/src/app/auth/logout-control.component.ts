import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-logout-control',
  templateUrl: './logout-control.component.html',
  styleUrl: './logout-control.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoutControlComponent {
  protected readonly auth = inject(AuthService);
}
