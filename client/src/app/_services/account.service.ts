import { JsonPipe } from '@angular/common';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators'
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';


class registerdto{
  


private _user : User;
public get user() : User {
  return this._user;
}
public set user(v : User) {
  this._user = v;
}


private _blob : object;
public get blob() : object {
  return this._blob;
}
public set blob(v : object) {
  this._blob = v;
}


}
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http :HttpClient){ }

  login(blob:any,model :any){
    const header = new HttpHeaders();
    header.append('Content-Type','multipart/form-data');
    header.append('Accept', 'multipart/form-data');
    const options =  {
        headers: header,
    };

    const formdata = new FormData();
    formdata.append("wavfile",blob);
    formdata.append("details",JSON.stringify(model));
    return this.http.post(this.baseUrl + 'account/login',formdata,options).pipe(
      map((response:any)=>{
        const user = response;
        if (user){
          this.setCurrentUser(user);
        }
      })
    );
  }

  
 
    register(blob:any,model:any){
    const header = new HttpHeaders();
    header.append('Content-Type','multipart/form-data');
    header.append('Accept', 'multipart/form-data');
    const options =  {
        headers: header,
    };

    const formdata = new FormData();
    formdata.append("wavfile",blob);
    formdata.append("details",JSON.stringify(model));

    return this.http.post<any>(this.baseUrl+'account/register',formdata,options).pipe(
      map((response :User) =>{
        const user = response;
        if (user){
          this.setCurrentUser(user);
        }
      })
    );
  }
  setCurrentUser(user: User){
    localStorage.setItem('user',JSON.stringify(user));
    this.currentUserSource.next(user);
  }
  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
