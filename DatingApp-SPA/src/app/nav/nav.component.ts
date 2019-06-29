import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};
  username: any = 'user';

  constructor(private authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  Login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.Success('Logged successfully!');
    }, error => {
      this.alertify.Error(error);
    });
  }

  LoggedIn() {
    this.username = this.authService.getUserName();
    return this.authService.loggedIn();
  }

  LogOut() {
    localStorage.removeItem('token');
    this.alertify.Message('Logged Out');
  }
}
