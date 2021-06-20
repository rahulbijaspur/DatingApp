import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
 @Input() username:string;
  @Input()messages :Message[]=[];
  @ViewChild('messageForm') messageForm:NgForm
messageContent:string;
  constructor(public messageService:MessageService) { }

  ngOnInit(): void {
    
  }
  sendMessage(){
    this.messageService.sendMessage(this.username,this.messageContent).then(() =>{
      this.messageForm.reset();
    })
  }


}
