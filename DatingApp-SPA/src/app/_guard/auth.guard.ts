import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  path: ActivatedRouteSnapshot[];
  route: ActivatedRouteSnapshot;

  constructor(private auth: AuthService, private router: Router,
              private alertify: AlertifyService) {}

  canActivate(): boolean {

    if(this.auth.loggedIn()) {
      return true;
    }

    this.alertify.Error('You shall not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}
