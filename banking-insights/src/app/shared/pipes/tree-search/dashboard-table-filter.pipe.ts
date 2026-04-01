import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dashboardTableFilter',
  pure: false
})

export class DashboardTableFilterPipe implements PipeTransform {

  transform(data: any, searchText: string): any {
    if (!data || !searchText?.trim()) return data;

    searchText = searchText.toLowerCase().trim();

const filterNode = (node: any): any | null => {
  const valuesToSearch: string[] = [];

  if (typeof node === 'string') {
    valuesToSearch.push(node);
  }

  if (node?.data) {
    if (node.data.label) valuesToSearch.push(node.data.label);
    if (node.data.subsidiary) valuesToSearch.push(node.data.subsidiary);
  }

  if (node?.label) valuesToSearch.push(node.label);
  if (node?.subsidiary) valuesToSearch.push(node.subsidiary);

  const isMatch = valuesToSearch
    .map(v => v.toString().toLowerCase().trim())
    .some(v => v.includes(searchText));

  const matchedChildren =
    node?.children
      ?.map(filterNode)
      .filter((c: any) => c !== null) || [];

  if (isMatch) {
    return {
      ...node,
      children: node?.children || [],
      expanded: true
    };
  }

  if (matchedChildren.length) {
    return {
      ...node,
      children: matchedChildren,
      expanded: true
    };
  }

  return null;
};



    if (Array.isArray(data)) {
      return data.map(filterNode).filter(n => n !== null);
    } else {
      const filteredRoot = filterNode(data);
      return filteredRoot ? [filteredRoot] : [];
    }
  }
}
