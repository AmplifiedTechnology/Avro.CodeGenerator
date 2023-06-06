# Avro.CodeGenerator

This source code generator will use Apache.Avro avrogen to create C# classes for all files with extension '.avro' marked with Build Action 'C# analyzer additional file'. 

The output file will be placed alongside the .avro file with the name template {avrofilename}.g.cs.  

Generated files will not be overwritten once created to allow for additional changes.  To regenerate the avro class, the previous generated file must be deleted and a rebuild triggered.  

The avro name and namespace properties will be maintained, however the generated class will receive the namesapce according to the folder structure and containing assembly.
