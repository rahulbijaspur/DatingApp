import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ToastrModule } from 'ngx-toastr';
import{TabsModule} from 'ngx-bootstrap/tabs';
import{NgxGalleryModule} from '@kolkov/ngx-gallery';
import "@angular/compiler";
import { FileUploadModule } from 'ng2-file-upload';
@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    TooltipModule,
    BsDropdownModule.forRoot(),
    TabsModule.forRoot(),
    NgxGalleryModule,
    FileUploadModule,
    ToastrModule.forRoot({
      positionClass:'toast-bottom-right'
    })
  ],
  exports:[
    TooltipModule,
    BsDropdownModule,
    ToastrModule,
    TabsModule,
    NgxGalleryModule,
    FileUploadModule

  ]
})
export class SharedModule { }
