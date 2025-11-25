using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
public static class Se4XmlImporter
{


}


// XML structure to import/export:
//<? xml version="1.0"?>
//<Settings>
//  <MultipleSearchAndReplaceList>
//    <Group>
//      <Name>Default</Name>
//      <Enabled>True</Enabled>
//      <MultipleSearchAndReplaceItem>
//        <Enabled>True</Enabled>
//        <FindWhat>a</FindWhat>
//        <ReplaceWith>e</ReplaceWith>
//        <SearchType>Normal</SearchType>
//        <Description />
//      </MultipleSearchAndReplaceItem>
//      <MultipleSearchAndReplaceItem>
//        <Enabled>True</Enabled>
//        <FindWhat>b</FindWhat>
//        <ReplaceWith>bb</ReplaceWith>
//        <SearchType>Normal</SearchType>
//        <Description />
//      </MultipleSearchAndReplaceItem>
//    </Group>
//    <Group>
//      <Name>group2</Name>
//      <Enabled>True</Enabled>
//      <MultipleSearchAndReplaceItem>
//        <Enabled>True</Enabled>
//        <FindWhat>33</FindWhat>
//        <ReplaceWith>333</ReplaceWith>
//        <SearchType>CaseSensitive</SearchType>
//        <Description>33</Description>
//      </MultipleSearchAndReplaceItem>
//    </Group>
//  </MultipleSearchAndReplaceList>
//</Settings>
