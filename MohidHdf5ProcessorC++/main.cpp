/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by The HDF Group.                                               *
 * Copyright by the Board of Trustees of the University of Illinois.         *
 * All rights reserved.                                                      *
 *                                                                           *
 * This file is part of HDF5.  The full HDF5 copyright notice, including     *
 * terms governing use, modification, and redistribution, is contained in    *
 * the files COPYING and Copyright.html.  COPYING can be found at the root   *
 * of the source code distribution tree; Copyright.html can be found at the  *
 * root level of an installed copy of the electronic HDF5 document set and   *
 * is linked from the top-level documents page.  It can also be found at     *
 * http://hdfgroup.org/HDF5/doc/Copyright.html.  If you do not have          *
 * access to either file, you may request a copy from help@hdfgroup.org.     *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 *   This example shows how to read data from a chunked dataset.
 *   We will read from the file created by extend.cpp
 */

#ifdef OLD_HEADER_FILENAME
#include <iostream.h>
#else
#include <iostream>
#endif

#include <string>

#ifndef H5_NO_NAMESPACE
#ifndef H5_NO_STD
	 using std::string;
    using std::cout;
    using std::endl;
#endif  // H5_NO_STD
#endif

#include "cpp\H5Cpp.h"

#ifndef H5_NO_NAMESPACE
    using namespace H5;
#endif

const int      NX = 359;
const int      NY = 718;
const int      RANK = 2;
const int      RANKC = 1;

int main (void)
{
	string file_name("E:\\Development\\MohidToolbox\\Stable\\x64\\Debug\\gfs.hdf5");
	string dataset_name ( "/Results/wind modulus/wind modulus_00001" );

    hsize_t	i, j;
	 float *data_out;  // buffer for dataset to be read
	 DataSet dataset;
	 DataSpace filespace;
	 DataSpace *mspace1;

	 try
	 {
		 data_out = new float[NX*NY]; //creates a new array of pointers to int objects

	 }
	 catch(...)
	 {
		 return -1;
	 }

    // Try block to detect exceptions raised by any of the calls inside it
    try
    {
		/*
		 * Turn off the auto-printing when failure occurs so that we can
		 * handle the errors appropriately
		 */
		Exception::dontPrint();

		H5File file( file_name.c_str(), H5F_ACC_RDONLY );

		for (int i = 0; i < 200000; i++)
		{

			dataset = file.openDataSet( dataset_name.c_str() );
			/*
			 * Get filespace for rank and dimension
			 */
			//filespace = dataset.getSpace();
			//cout << "space opened" << "\n";
			///*
			// * Get number of dimensions in the file dataspace
			// */
			//int rank = filespace.getSimpleExtentNdims();

			///*
			// * Get and print the dimension sizes of the file dataspace
			// */
			//hsize_t dims[2]; 	// dataset dimensions
			//rank = filespace.getSimpleExtentDims( dims );
			//cout << "dataset rank = " << rank << ", dimensions "
			//	  << (unsigned long)(dims[0]) << " x "
			//	  << (unsigned long)(dims[1]) << endl;

			/*
			 * Define the memory space to read dataset.
			 */
			//mspace1 = new DataSpace(RANK, dims);
			//cout << "memory space created" << "\n";

			/*
			 * Read dataset back and display.
			 */			
			dataset.read(data_out, PredType::NATIVE_FLOAT);			

			dataset.close();
		}

    }  // end of try block

    // catch failure caused by the H5File operations
    catch( FileIException error )
    {
		error.printError();
		return -1;
    }

    // catch failure caused by the DataSet operations
    catch( DataSetIException error )
    {
		error.printError();
		return -1;
    }

    // catch failure caused by the DataSpace operations
    catch( DataSpaceIException error )
    {
		error.printError();
		return -1;
    }
    return 0;
}
