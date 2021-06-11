import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import * as RecordRTC from 'recordrtc';
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  model :any={};

  @Output() cancelRegister = new EventEmitter();
  constructor(private domSanitizer: DomSanitizer,
     public router :Router,
     private accountService:AccountService,
     private toastr:ToastrService) { }

  ngOnInit() {
  }

  register(){
    this.accountService.register(this.blob,this.model).subscribe(response=>{
      console.log(response);
      this.cancel();
    },error=>{
      console.log(error);
      this.toastr.error(error.error)
    })
  }
  cancel(){
    this.cancelRegister.emit(false);
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
  console.log(this.record.buffer);

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
