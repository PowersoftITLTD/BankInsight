import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'mspSearchPipe',
  pure: false 
})
export class MspSearchPipePipe implements PipeTransform {

  transform(value: any[], search: string, fields:string[] = []):any {
    if (!value || !search) return value;


    search = search.toLowerCase();

    if(fields.length === 0){
      return value.filter(row => JSON.stringify(row).toLowerCase().includes(search))
    }

    return value.filter(row => {
      return fields.some(feild => {
        const value = row[feild]?.toString().toLowerCase();
        return value && value.includes(search); 
      })
    })

  }

}
