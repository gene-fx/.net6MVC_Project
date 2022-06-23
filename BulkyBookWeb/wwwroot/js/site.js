// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function Delete(url)
{
	Swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		color: '#fff',
		background: '#000000',
		icon: 'question',
		iconColor: '#e84c3d',
		showCancelButton: true,
		confirmButtonColor: '#365c7c',
		cancelButtonColor: '#e84c3d',
		confirmButtonText: 'Yes, delete it!'
	}).then((result) => {
		if (result.isConfirmed) {
			$.ajax({
				url: url,
				type: 'DELETE',
				success: function (data) {
					if (data.success) {
						toastr.success(data.message);
						$(location).attr('href', data.href);
					}
					else {
						toastr.error(data.message);
					}
				}
			})

		}
	})
}