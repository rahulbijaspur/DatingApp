import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { User } from '../_models/User';
import { AccountService } from '../_services/account.service';
import { DomSanitizer } from '@angular/platform-browser';
import * as RecordRTC from 'recordrtc';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model ={
    "username":"",
    "password":""
  }

  
  constructor(public accountService :AccountService,private router :Router,private toastr:ToastrService,
    private domSanitizer: DomSanitizer) { }
 
  ngOnInit():void {

  }

  login(){
    this.accountService.login(this.blob,this.model).subscribe(
      response =>{
      this.router.navigateByUrl('/members');
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  
  title = 'sample';
  record:any;
  recording = false;
  recordended=false;
  url:any;
  error:any;
  static emailid:any;
  static loading = false;
  blob:Blob;

  sanitize(url: string) {
    return this.domSanitizer.bypassSecurityTrustUrl(url);
  }
  initiateRecording() {
    this.recording = true;
    let mediaConstraints = {
      video: false,
      audio: true
    };
    navigator.mediaDevices.getUserMedia(mediaConstraints).then(this.successCallback.bind(this), this.errorCallback.bind(this));
  }

 successCallback(stream:any) {
  var options = {
    mimeType: "audio/wav",
    numberOfAudioChannels: 1,
    sampleRate: 44000,
  };
  var StereoAudioRecorder = RecordRTC.StereoAudioRecorder;
  this.record= new StereoAudioRecorder(stream, options);
  this.record.record();
  // console.log(this.record.buffer);

}

processRecording(blob: any) {
  this.url = URL.createObjectURL(blob);
  this.blob =blob;
}

  stopRecording() {
    this.recording = false;
    this.record.stop(this.processRecording.bind(this));
  }

  errorCallback(error:any) {
    this.error = 'Can not play audio in your browser';
  }

}
