import { Injectable } from '@angular/core';
declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

  constructor() { }

  Confirm(message: string, okCallback: () => any) {
    alertify.confirm(message, (e) => {
      if (e) {
        okCallback();
      } else {}
    });
  }

  Success(message: string) {
    alertify.success(message);
  }

  Error(message: string) {
    alertify.error(message);
  }

  Warning(message: string) {
    alertify.warning(message);
  }

  Message(message: string) {
    alertify.message(message);
  }

}
