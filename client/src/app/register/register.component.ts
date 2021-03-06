import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';
import * as RecordRTC from 'recordrtc';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  model :any={};
  @Output() cancelRegister = new EventEmitter();
  registerForm :FormGroup;
  maxDate:Date;
  validationErrors: string[]=[];
  ALPHA_NUMERIC_REGEX = /^[a-zA-Z0-9_]*$/;
  ALPHA_NUMERIC_VALIDATION_ERROR = { alphaNumericError: 'only alpha numeric values are allowed' }
  
  

  constructor(private domSanitizer: DomSanitizer,
     public router :Router,
     private accountService:AccountService,
     private toastr:ToastrService,
     private fb : FormBuilder) { }

  ngOnInit() {
    this.initializeForm();
    this.maxDate=new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);

  }
  
  initializeForm(){
    this.registerForm = this.fb.group({
      gender:['male'],
      username :['',Validators.required],
      knownAs :['',Validators.required],
      dateOfBirth :['',Validators.required],
      city:['Unknown'],
      country :['Unknown'],
      password:['',[Validators.required,Validators.minLength(6),Validators.maxLength(12)]],
      confirmPassword:['',[Validators.required,this.matchValues("password")]]
    })
    this.registerForm.controls.password.valueChanges.subscribe(()=>{
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }


  matchValues(matchTo:string):ValidatorFn{
    return (control:AbstractControl)=>{
      return control?.value===control?.parent?.controls[matchTo].value?null:{isMatching:true} 
    }
  }
  alphaNumericValidator(control: FormControl): ValidationErrors | null {
    return this.ALPHA_NUMERIC_REGEX.test(control.value) ? null : this.ALPHA_NUMERIC_VALIDATION_ERROR;
}


  register(){
    // console.log(this.registerForm.value);
    this.accountService.register(this.blob,this.registerForm.value).subscribe(response=>{
      this.router.navigateByUrl('/members');
    },error=>{
      this.toastr.show(error[0].description);
      console.log(error);
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
