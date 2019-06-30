import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};
  username: any = 'user';

  constructor(private authService: AuthService, private alertify: AlertifyService,
              private router: Router) { }

  ngOnInit() {
  }

  Login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.Success('Logged successfully!');
    }, error => {
      this.alertify.Error(error);
    }, () => {
      this.router.navigate(['/members']);
    });
  }

  LoggedIn() {
    this.username = this.authService.getUserName();
    return this.authService.loggedIn();
  }

  LogOut() {
    localStorage.removeItem('token');
    this.router.navigate(['/home']);
    this.alertify.Message('Logged Out');
  }
}
