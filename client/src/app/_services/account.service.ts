import { JsonPipe } from '@angular/common';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators'
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';
import { PresenceService } from './presence.service';


@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http :HttpClient,private presence :PresenceService){ }

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
          this.presence.createHubConnection(user);
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
          this.presence.createHubConnection(user);
        }
      })
    );
  }
  setCurrentUser(user: User){
    user.roles=[];
    const roles =this.getDecodedToken(user.token).role;
    Array.isArray(roles)? user.roles =roles : user.roles.push(roles);
    localStorage.setItem('user',JSON.stringify(user));
    this.currentUserSource.next(user);
  }
  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presence.stopHubConnection();
  }

  getDecodedToken(token){
    return JSON.parse(atob(token.split('.')[1]));

  }
}
